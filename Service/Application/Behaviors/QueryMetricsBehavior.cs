using Microsoft.Extensions.Logging;
using Application.Abstractions;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Application.Behaviors;

/// <summary>
/// Pipeline behavior that collects business metrics for queries.
/// </summary>
public class QueryMetricsBehavior<TQuery, TQueryResult> : IQueryPipelineBehavior<TQuery, TQueryResult>
    where TQueryResult : class
{
    private readonly ILogger<QueryMetricsBehavior<TQuery, TQueryResult>> _logger;
    private readonly IMetricsService _metrics;
    private readonly IResultAnalyzer _resultAnalyzer;

    private static readonly ActivitySource ActivitySource = new("Orange6.Application.Queries");
    private static readonly Meter Meter = new("Orange6.Application.Queries");
    private static readonly Counter<long> QueryAttempts = Meter.CreateCounter<long>(
        "orange6.query.attempts", "attempts", "Number of query attempts");
    private static readonly Counter<long> QuerySuccess = Meter.CreateCounter<long>(
        "orange6.query.success", "successes", "Number of successful queries");
    private static readonly Counter<long> QueryNotFound = Meter.CreateCounter<long>(
        "orange6.query.not_found", "not_found", "Number of queries with no results");
    private static readonly Counter<long> QueryExceptions = Meter.CreateCounter<long>(
        "orange6.query.exceptions", "exceptions", "Number of query exceptions");
    private static readonly Histogram<int> QueryResultCount = Meter.CreateHistogram<int>(
        "orange6.query.result_count", "items", "Number of items returned by queries");

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
        var queryName = typeof(TQuery).Name.Replace("Query", "").ToLowerInvariant();

        using var activity = ActivitySource.StartActivity($"Query.{queryName}", ActivityKind.Internal);
        activity?.SetTag("query.type", queryName);
        activity?.SetTag("query.fullname", typeof(TQuery).FullName);
        activity?.SetTag("operation", "query");

        var tags = new Dictionary<string, string>
        {
            ["QueryType"] = queryName,
            ["Operation"] = "query"
        };

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

    private void RecordAttemptMetrics(string queryName, Dictionary<string, string> tags, Activity? activity)
    {
        _metrics.RecordCounter($"{queryName}_attempts", tags);
        _metrics.RecordCounter("query_attempts_total", tags);

        var metricTags = new TagList { { "query.type", queryName }, { "operation", "query" } };
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
            QuerySuccess.Add(1, metricTags);
        else
            QueryNotFound.Add(1, metricTags);

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
        activity?.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection
        {
            { "exception.type", ex.GetType().FullName },
            { "exception.message", ex.Message }
        }));

        _logger.LogWarning("BUSINESS_METRIC: Query {QueryName} threw {ExceptionType}", queryName, ex.GetType().Name);
    }
}
