using Microsoft.Extensions.Logging;
using Application.Abstractions;
using System.Diagnostics;

namespace Application.Behaviors;

/// <summary>
/// Pipeline behavior that monitors performance and logs warnings for slow-running queries.
/// </summary>
public class QueryPerformanceBehavior<TQuery, TQueryResult> : IQueryPipelineBehavior<TQuery, TQueryResult>
{
    private readonly ILogger<QueryPerformanceBehavior<TQuery, TQueryResult>> _logger;
    private readonly TimeSpan _slowQueryThreshold;

    public QueryPerformanceBehavior(
        ILogger<QueryPerformanceBehavior<TQuery, TQueryResult>> logger,
        TimeSpan? slowQueryThreshold = null)
    {
        _logger = logger;
        _slowQueryThreshold = slowQueryThreshold ?? TimeSpan.FromSeconds(3);
    }

    public async Task<TQueryResult> Handle(TQuery query, CancellationToken cancellationToken, Func<Task<TQueryResult>> next)
    {
        var queryName = typeof(TQuery).Name;
        var stopwatch = Stopwatch.StartNew();
        var succeeded = false;

        try
        {
            var result = await next();
            stopwatch.Stop();
            succeeded = true;

            _logger.LogDebug("Query performance: {QueryName} executed in {ElapsedMilliseconds}ms. Query: {@Query}",
                queryName, stopwatch.ElapsedMilliseconds, query);

            if (stopwatch.Elapsed > _slowQueryThreshold)
            {
                _logger.LogWarning("Slow query detected: {QueryName} took {ElapsedMilliseconds}ms (threshold: {ThresholdMilliseconds}ms). Query: {@Query}",
                    queryName, stopwatch.ElapsedMilliseconds, _slowQueryThreshold.TotalMilliseconds, query);
            }

            return result;
        }
        finally
        {
            if (!succeeded)
            {
                stopwatch.Stop();
                _logger.LogDebug("Query failed: {QueryName} executed in {ElapsedMilliseconds}ms before exception. Query: {@Query}",
                    queryName, stopwatch.ElapsedMilliseconds, query);
            }
        }
    }
}
