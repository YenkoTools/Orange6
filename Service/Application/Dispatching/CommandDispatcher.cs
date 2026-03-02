using Microsoft.Extensions.DependencyInjection;
using Orange6.Application.Abstractions;

namespace Orange6.Application.Dispatching;

/// <summary>
/// Dispatcher for commands that supports pipeline behaviors for cross-cutting concerns.
/// </summary>
public class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    public async Task<TCommandResult> Dispatch<TCommand, TCommandResult>(TCommand command, CancellationToken cancellationToken)
    {
        var behaviors = serviceProvider.GetServices<ICommandPipelineBehavior<TCommand, TCommandResult>>().ToArray();

        Func<Task<TCommandResult>> handlerFunc = async () =>
        {
            var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TCommandResult>>();
            return await handler.Handle(command, cancellationToken);
        };

        for (int i = behaviors.Length - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var nextFunc = handlerFunc;
            handlerFunc = () => behavior.Handle(command, cancellationToken, nextFunc);
        }

        return await handlerFunc();
    }
}
