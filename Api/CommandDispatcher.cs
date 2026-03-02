using Api.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Api;

/// <summary>
/// Dispatcher for commands that supports pipeline behaviors for cross-cutting concerns.
/// </summary>
public class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task<TCommandResult> Dispatch<TCommand, TCommandResult>(TCommand command, CancellationToken cancellation)
    {
        // Get all pipeline behaviors for this command type
        var behaviors = _serviceProvider.GetServices<ICommandPipelineBehavior<TCommand, TCommandResult>>().ToArray();

        // Create the pipeline execution function
        Func<Task<TCommandResult>> handlerFunc = async () =>
        {
            var handler = _serviceProvider.GetRequiredService<ICommandHandler<TCommand, TCommandResult>>();
            return await handler.Handle(command, cancellation);
        };

        // Execute behaviors in reverse order to create proper nesting
        for (int i = behaviors.Length - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var nextFunc = handlerFunc;
            handlerFunc = () => behavior.Handle(command, cancellation, nextFunc);
        }

        return await handlerFunc();
    }
}