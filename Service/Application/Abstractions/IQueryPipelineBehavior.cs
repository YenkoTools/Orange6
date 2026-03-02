namespace Orange6.Application.Abstractions;

/// <summary>
/// Represents a pipeline behavior that can intercept and process queries.
/// </summary>
public interface IQueryPipelineBehavior<in TQuery, TQueryResult>
{
    Task<TQueryResult> Handle(TQuery query, CancellationToken cancellationToken, Func<Task<TQueryResult>> next);
}
