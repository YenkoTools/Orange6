using Api.Common;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Behaviors;

/// <summary>
/// Pipeline behavior that monitors performance and logs warnings for slow-running commands.
/// </summary>
/// <typeparam name="TCommand">The type of command being processed.</typeparam>
/// <typeparam name="TCommandResult">The type of result returned by the command.</typeparam>
public class CommandPerformanceBehavior<TCommand, TCommandResult> : ICommandPipelineBehavior<TCommand, TCommandResult>
{
    private readonly ILogger<CommandPerformanceBehavior<TCommand, TCommandResult>> _logger;
    private readonly TimeSpan _slowCommandThreshold;

    public CommandPerformanceBehavior(ILogger<CommandPerformanceBehavior<TCommand, TCommandResult>> logger,
        TimeSpan? slowCommandThreshold = null)
    {
        _logger = logger;
        _slowCommandThreshold = slowCommandThreshold ?? TimeSpan.FromSeconds(5); // Default 5 seconds for commands
    }

    public async Task<TCommandResult> Handle(TCommand command, CancellationToken cancellationToken, Func<Task<TCommandResult>> next)
    {
        var commandName = typeof(TCommand).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await next();

            stopwatch.Stop();

            // Always log performance data at Debug level
            _logger.LogDebug("Command performance: {CommandName} executed in {ElapsedMilliseconds}ms. Command: {@Command}",
                commandName, stopwatch.ElapsedMilliseconds, command);

            // Log warning for slow commands
            if (stopwatch.Elapsed > _slowCommandThreshold)
            {
                _logger.LogWarning("Slow command detected: {CommandName} took {ElapsedMilliseconds}ms (threshold: {ThresholdMilliseconds}ms). Command: {@Command}",
                    commandName, stopwatch.ElapsedMilliseconds, _slowCommandThreshold.TotalMilliseconds, command);
            }

            return result;
        }
        catch (Exception)
        {
            stopwatch.Stop();
            _logger.LogDebug("Command failed: {CommandName} executed in {ElapsedMilliseconds}ms before exception. Command: {@Command}",
                commandName, stopwatch.ElapsedMilliseconds, command);
            throw;
        }
    }
}
