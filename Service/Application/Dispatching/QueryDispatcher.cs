using Microsoft.Extensions.DependencyInjection;
using Orange6.Application.Abstractions;

namespace Orange6.Application.Dispatching;

/// <summary>
/// Dispatcher for queries that supports pipeline behaviors for cross-cutting concerns.
/// </summary>
public class QueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
    public async Task<TQueryResult> Dispatch<TQuery, TQueryResult>(TQuery query, CancellationToken cancellationToken)
    {
        var behaviors = serviceProvider.GetServices<IQueryPipelineBehavior<TQuery, TQueryResult>>().ToArray();

        Func<Task<TQueryResult>> handlerFunc = async () =>
        {
            var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TQueryResult>>();
            return await handler.Handle(query, cancellationToken);
        };

        for (int i = behaviors.Length - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var nextFunc = handlerFunc;
            handlerFunc = () => behavior.Handle(query, cancellationToken, nextFunc);
        }

        return await handlerFunc();
    }
}
