namespace Api.Common;

/// <summary>
/// Represents a pipeline behavior that can intercept and process commands.
/// </summary>
/// <typeparam name="TCommand">The type of command being processed.</typeparam>
/// <typeparam name="TCommandResult">The type of result returned by the command.</typeparam>
public interface ICommandPipelineBehavior<in TCommand, TCommandResult>
{
    /// <summary>
    /// Handles the command and invokes the next behavior in the pipeline.
    /// </summary>
    /// <param name="command">The command being processed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="next">The next behavior in the pipeline.</param>
    /// <returns>The result of the command processing.</returns>
    Task<TCommandResult> Handle(TCommand command, CancellationToken cancellationToken, Func<Task<TCommandResult>> next);
}
