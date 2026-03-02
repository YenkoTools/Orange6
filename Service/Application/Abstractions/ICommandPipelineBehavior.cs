namespace Application.Abstractions;

/// <summary>
/// Represents a pipeline behavior that can intercept and process commands.
/// </summary>
public interface ICommandPipelineBehavior<in TCommand, TCommandResult>
{
    Task<TCommandResult> Handle(TCommand command, CancellationToken cancellationToken, Func<Task<TCommandResult>> next);
}
