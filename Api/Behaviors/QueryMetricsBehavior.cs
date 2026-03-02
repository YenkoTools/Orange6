using Api.Common;
using Api.Domain.Common;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace Api.Behaviors;

/// <summary>
/// Pipeline behavior that collects business metrics for queries using OpenTelemetry.
/// Complements QueryPerformanceBehavior by focusing on success/failure rates and business intelligence.
/// </summary>
/// <typeparam name="TQuery">The type of query being processed.</typeparam>
/// <typeparam name="TQueryResult">The type of result returned by the query.</typeparam>
public class QueryMetricsBehavior<TQuery, TQueryResult> : IQueryPipelineBehavior<TQuery, TQueryResult>
    where TQueryResult : class
{
    private readonly ILogger<QueryMetricsBehavior<TQuery, TQueryResult>> _logger;
    private readonly IMetricsService _metrics;
    private readonly IResultAnalyzer _resultAnalyzer;

    // OpenTelemetry instrumentation
    private static readonly ActivitySource ActivitySource = new("IronAmbit.Application.Queries");
    private static readonly Meter Meter = new("IronAmbit.Application.Queries");
    private static readonly Counter<long> QueryAttempts = Meter.CreateCounter<long>(
        "ironambit.query.attempts",
        "attempts",
        "Number of query attempts");
    private static readonly Counter<long> QuerySuccess = Meter.CreateCounter<long>(
        "ironambit.query.success",
        "successes",
        "Number of successful queries");
    private static readonly Counter<long> QueryNotFound = Meter.CreateCounter<long>(
        "ironambit.query.not_found",
        "not_found",
        "Number of queries with no results");
    private static readonly Counter<long> QueryExceptions = Meter.CreateCounter<long>(
        "ironambit.query.exceptions",
        "exceptions",
        "Number of query exceptions");
    private static readonly Histogram<int> QueryResultCount = Meter.CreateHistogram<int>(
        "ironambit.query.result_count",
        "items",
        "Number of items returned by queries");

    public QueryMetricsBehavior(
        ILogger<QueryMetricsBehavior<TQuery, TQueryResult>> logger,
        IMetricsService metrics,
        IResultAnalyzer? resultAnalyzer = null)
    {
        _logger = logger;
        _metrics = metrics;
        _resultAnalyzer = resultAnalyzer ?? new DefaultResultAnalyzer();
    }

    public async Task<TQueryResult> Handle(TQuery query, CancellationToken cancellationToken, Func<Task<TQueryResult>> next)
    {
        var queryName = GetQueryName();

        // Create OpenTelemetry activity (span)
        using var activity = ActivitySource.StartActivity($"Query.{queryName}", ActivityKind.Internal);
        activity?.SetTag("query.type", queryName);
        activity?.SetTag("query.fullname", typeof(TQuery).FullName);
        activity?.SetTag("operation", "query");

        var tags = CreateBaseTags(queryName);

        try
        {
            RecordAttemptMetrics(queryName, tags, activity);

            var result = await next();

            RecordSuccessMetrics(result, queryName, tags, activity);

            return result;
        }
        catch (Exception ex)
        {
            RecordExceptionMetrics(queryName, tags, ex, activity);
            throw;
        }
    }

    private static string GetQueryName()
    {
        return typeof(TQuery).Name.Replace("Query", "").ToLowerInvariant();
    }

    private static Dictionary<string, string> CreateBaseTags(string queryName)
    {
        return new Dictionary<string, string>
        {
            ["QueryType"] = queryName,
            ["Operation"] = "query"
        };
    }

    private void RecordAttemptMetrics(string queryName, Dictionary<string, string> tags, Activity? activity)
    {
        _metrics.RecordCounter($"{queryName}_attempts", tags);
        _metrics.RecordCounter("query_attempts_total", tags);

        var metricTags = new TagList
        {
            { "query.type", queryName },
            { "operation", "query" }
        };
        QueryAttempts.Add(1, metricTags);

        _logger.LogDebug("Recording metrics for query attempt: {QueryName}", queryName);
        activity?.AddEvent(new ActivityEvent("QueryAttempt"));
    }

    private void RecordSuccessMetrics(TQueryResult result, string queryName, Dictionary<string, string> tags, Activity? activity)
    {
        var analysisResult = _resultAnalyzer.AnalyzeResult(result);
        var outcome = analysisResult.IsSuccess ? "success" : "not_found";

        tags.Add("Outcome", outcome);
        tags.Add("ResultType", analysisResult.ResultType);

        _metrics.RecordCounter($"{queryName}_{outcome}", tags);
        _metrics.RecordCounter($"query_{outcome}_total", tags);

        var metricTags = new TagList
        {
            { "query.type", queryName },
            { "operation", "query" },
            { "outcome", outcome },
            { "result.type", analysisResult.ResultType }
        };

        if (analysisResult.IsSuccess)
        {
            QuerySuccess.Add(1, metricTags);
        }
        else
        {
            QueryNotFound.Add(1, metricTags);
        }

        activity?.SetTag("query.outcome", outcome);
        activity?.SetTag("query.result_type", analysisResult.ResultType);
        activity?.SetStatus(analysisResult.IsSuccess ? ActivityStatusCode.Ok : ActivityStatusCode.Error, outcome);

        if (analysisResult.ResultType == "collection" && analysisResult.ItemCount.HasValue)
        {
            tags.Add("ItemCount", analysisResult.ItemCount.Value.ToString());
            _metrics.RecordHistogram($"{queryName}_result_count", analysisResult.ItemCount.Value, tags);
            _metrics.RecordHistogram("query_result_count", analysisResult.ItemCount.Value, tags);

            metricTags.Add("item.count", analysisResult.ItemCount.Value);
            QueryResultCount.Record(analysisResult.ItemCount.Value, metricTags);

            activity?.SetTag("query.item_count", analysisResult.ItemCount.Value);
        }

        _logger.LogInformation("BUSINESS_METRIC: Query {QueryName} completed with outcome: {Outcome}, ResultType: {ResultType}",
            queryName, outcome, analysisResult.ResultType);
    }

    private void RecordExceptionMetrics(string queryName, Dictionary<string, string> tags, Exception ex, Activity? activity)
    {
        tags.Add("Outcome", "exception");
        tags.Add("ExceptionType", ex.GetType().Name);

        _metrics.RecordCounter($"{queryName}_exceptions", tags);
        _metrics.RecordCounter("query_exceptions_total", tags);

        var metricTags = new TagList
        {
            { "query.type", queryName },
            { "operation", "query" },
            { "outcome", "exception" },
            { "exception.type", ex.GetType().Name }
        };
        QueryExceptions.Add(1, metricTags);

        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.AddException(ex);

        _logger.LogWarning("BUSINESS_METRIC: Query {QueryName} threw {ExceptionType}",
            queryName, ex.GetType().Name);
    }
}

/// <summary>
/// Interface for analyzing query results
/// </summary>
public interface IResultAnalyzer
{
    ResultAnalysis AnalyzeResult(object? result);
}

/// <summary>
/// Result of analyzing a query result
/// </summary>
public record ResultAnalysis(bool IsSuccess, string ResultType, int? ItemCount);

/// <summary>
/// Default implementation of result analyzer
/// </summary>
public class DefaultResultAnalyzer : IResultAnalyzer
{
    public ResultAnalysis AnalyzeResult(object? result)
    {
        if (result == null)
        {
            return new ResultAnalysis(false, "null", null);
        }

        var resultType = result.GetType();

        // Handle Result<T> pattern
        if (IsResultType(resultType))
        {
            return AnalyzeResultPattern(result, resultType);
        }

        // Handle other success patterns
        if (HasSuccessProperty(result, resultType, out var isSuccess))
        {
            var innerResultType = isSuccess ? DetermineResultType(result) : "result_failure";
            return new ResultAnalysis(isSuccess, innerResultType, null);
        }

        // Direct result analysis
        var directResultType = DetermineResultType(result);
        var itemCount = directResultType == "collection" ? GetCollectionCount(result) : null;
        return new ResultAnalysis(true, directResultType, itemCount);
    }

    private static bool IsResultType(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>);
    }

    private ResultAnalysis AnalyzeResultPattern(object result, Type resultType)
    {
        var isSuccess = GetPropertyValue<bool>(result, resultType, "IsSuccess");

        if (isSuccess)
        {
            var value = GetPropertyValue<object>(result, resultType, "Value");
            var innerResultType = DetermineResultType(value);
            var itemCount = innerResultType == "collection" ? GetCollectionCount(result) : null;
            return new ResultAnalysis(true, innerResultType, itemCount);
        }

        return new ResultAnalysis(false, "result_failure", null);
    }

    private static bool HasSuccessProperty(object obj, Type type, out bool isSuccess)
    {
        isSuccess = false;

        if (!type.Name.Contains("Result"))
        {
            return false;
        }

        var property = type.GetProperty("IsSuccess") ??
                      type.GetProperty("Success") ??
                      type.GetProperty("Succeeded");

        if (property == null)
        {
            return false;
        }

        isSuccess = (bool)(property.GetValue(obj) ?? false);
        return true;
    }

    private static T? GetPropertyValue<T>(object obj, Type type, string propertyName)
    {
        var property = type.GetProperty(propertyName);
        if (property == null)
        {
            return default;
        }

        var value = property.GetValue(obj);
        return value is T typedValue ? typedValue : default;
    }

    private static string DetermineResultType(object? value)
    {
        if (value == null)
        {
            return "null";
        }

        var type = value.GetType();

        // Check if it's a collection (but not a string)
        if (IsCollection(type))
        {
            return "collection";
        }

        // Check common primitive types
        if (IsPrimitiveType(type))
        {
            return "primitive";
        }

        // Default to entity/object
        return "entity";
    }

    private static bool IsCollection(Type type)
    {
        return typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string);
    }

    private static bool IsPrimitiveType(Type type)
    {
        return type.IsPrimitive ||
               type == typeof(string) ||
               type == typeof(Guid) ||
               type == typeof(DateTime) ||
               type == typeof(decimal);
    }

    private static int? GetCollectionCount(object? result)
    {
        if (result == null)
        {
            return null;
        }

        var resultType = result.GetType();

        // Handle Result<IEnumerable<T>> pattern
        if (IsResultType(resultType))
        {
            var value = GetPropertyValue<object>(result, resultType, "Value");
            return CountEnumerable(value);
        }

        // Direct enumerable
        return CountEnumerable(result);
    }

    private static int? CountEnumerable(object? obj)
    {
        if (obj is System.Collections.IEnumerable enumerable and not string)
        {
            return enumerable.Cast<object>().Count();
        }

        return null;
    }
}
