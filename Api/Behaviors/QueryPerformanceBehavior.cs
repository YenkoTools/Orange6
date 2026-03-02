using Api.Common;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Api.Behaviors;

/// <summary>
/// Pipeline behavior that monitors performance and logs warnings for slow-running queries.
/// </summary>
/// <typeparam name="TQuery">The type of query being processed.</typeparam>
/// <typeparam name="TQueryResult">The type of result returned by the query.</typeparam>
public class QueryPerformanceBehavior<TQuery, TQueryResult> : IQueryPipelineBehavior<TQuery, TQueryResult>
{
    private readonly ILogger<QueryPerformanceBehavior<TQuery, TQueryResult>> _logger;
    private readonly TimeSpan _slowQueryThreshold;

    public QueryPerformanceBehavior(ILogger<QueryPerformanceBehavior<TQuery, TQueryResult>> logger,
        TimeSpan? slowQueryThreshold = null)
    {
        _logger = logger;
        _slowQueryThreshold = slowQueryThreshold ?? TimeSpan.FromSeconds(3); // Default 3 seconds
    }

    public async Task<TQueryResult> Handle(TQuery query, CancellationToken cancellationToken, Func<Task<TQueryResult>> next)
    {
        var queryName = typeof(TQuery).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await next();

            stopwatch.Stop();

            // Always log performance data at Debug level
            _logger.LogDebug("Query performance: {QueryName} executed in {ElapsedMilliseconds}ms. Query: {@Query}",
                queryName, stopwatch.ElapsedMilliseconds, query);

            // Log warning for slow queries
            if (stopwatch.Elapsed > _slowQueryThreshold)
            {
                _logger.LogWarning("Slow query detected: {QueryName} took {ElapsedMilliseconds}ms (threshold: {ThresholdMilliseconds}ms). Query: {@Query}",
                    queryName, stopwatch.ElapsedMilliseconds, _slowQueryThreshold.TotalMilliseconds, query);
            }

            return result;
        }
        catch (Exception)
        {
            stopwatch.Stop();
            _logger.LogDebug("Query failed: {QueryName} executed in {ElapsedMilliseconds}ms before exception. Query: {@Query}",
                queryName, stopwatch.ElapsedMilliseconds, query);
            throw;
        }
    }
}
