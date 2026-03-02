using Api.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Api;

/// <summary>
/// Dispatcher for queries that supports pipeline behaviors for cross-cutting concerns.
/// </summary>
public class QueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<TQueryResult> Dispatch<TQuery, TQueryResult>(TQuery query, CancellationToken cancellationToken)
    {
        // Get all pipeline behaviors for this query type
        var behaviors = _serviceProvider.GetServices<IQueryPipelineBehavior<TQuery, TQueryResult>>().ToArray();

        // Create the pipeline execution function
        Func<Task<TQueryResult>> handlerFunc = async () =>
        {
            var handler = _serviceProvider.GetRequiredService<IQueryHandler<TQuery, TQueryResult>>();
            return await handler.Handle(query, cancellationToken);
        };

        // Execute behaviors in reverse order to create proper nesting
        for (int i = behaviors.Length - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var nextFunc = handlerFunc;
            handlerFunc = () => behavior.Handle(query, cancellationToken, nextFunc);
        }

        return await handlerFunc();
    }
}
