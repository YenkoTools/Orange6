namespace Api.Common;

/// <summary>
/// Represents a pipeline behavior that can intercept and process queries.
/// </summary>
/// <typeparam name="TQuery">The type of query being processed.</typeparam>
/// <typeparam name="TQueryResult">The type of result returned by the query.</typeparam>
public interface IQueryPipelineBehavior<in TQuery, TQueryResult>
{
    /// <summary>
    /// Handles the query and invokes the next behavior in the pipeline.
    /// </summary>
    /// <param name="query">The query being processed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="next">The next behavior in the pipeline.</param>
    /// <returns>The result of the query processing.</returns>
    Task<TQueryResult> Handle(TQuery query, CancellationToken cancellationToken, Func<Task<TQueryResult>> next);
}
