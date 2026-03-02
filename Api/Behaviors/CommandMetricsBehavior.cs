using Api.Common;
using Api.Domain.Common;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Api.Behaviors;

/// <summary>
/// Pipeline behavior that collects business metrics for commands using OpenTelemetry.
/// Complements CommandPerformanceBehavior by focusing on success/failure rates and business intelligence.
/// </summary>
/// <typeparam name="TCommand">The type of command being processed.</typeparam>
/// <typeparam name="TCommandResult">The type of result returned by the command.</typeparam>
public class CommandMetricsBehavior<TCommand, TCommandResult> : ICommandPipelineBehavior<TCommand, TCommandResult>
    where TCommandResult : class
{
    private readonly ILogger<CommandMetricsBehavior<TCommand, TCommandResult>> _logger;
    private readonly IMetricsService _metrics;

    // OpenTelemetry instrumentation
    private static readonly ActivitySource ActivitySource = new("Orange6.Api.Commands");
    private static readonly Meter Meter = new("Orange6.Api.Commands");
    private static readonly Counter<long> CommandAttempts = Meter.CreateCounter<long>(
        "orange6.command.attempts",
        "attempts",
        "Number of command attempts");
    private static readonly Counter<long> CommandSuccess = Meter.CreateCounter<long>(
        "orange6.command.success",
        "successes",
        "Number of successful commands");
    private static readonly Counter<long> CommandFailures = Meter.CreateCounter<long>(
        "orange6.command.failures",
        "failures",
        "Number of failed commands");
    private static readonly Counter<long> CommandExceptions = Meter.CreateCounter<long>(
        "orange6.command.exceptions",
        "exceptions",
        "Number of command exceptions");

    public CommandMetricsBehavior(
        ILogger<CommandMetricsBehavior<TCommand, TCommandResult>> logger,
        IMetricsService metrics)
    {
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<TCommandResult> Handle(TCommand command, CancellationToken cancellationToken, Func<Task<TCommandResult>> next)
    {
        var commandName = typeof(TCommand).Name.Replace("Command", "").ToLowerInvariant();

        // Create OpenTelemetry activity (span)
        using var activity = ActivitySource.StartActivity($"Command.{commandName}", ActivityKind.Internal);
        activity?.SetTag("command.type", commandName);
        activity?.SetTag("command.fullname", typeof(TCommand).FullName);
        activity?.SetTag("operation", "command");

        var tags = new Dictionary<string, string>
        {
            ["CommandType"] = commandName,
            ["Operation"] = "command"
        };

        // OpenTelemetry metric tags
        var metricTags = new TagList
        {
            { "command.type", commandName },
            { "operation", "command" }
        };

        try
        {
            // Record both legacy and OpenTelemetry metrics
            _metrics.RecordCounter($"{commandName}_attempts", tags);
            _metrics.RecordCounter("command_attempts_total", tags);
            CommandAttempts.Add(1, metricTags);

            _logger.LogDebug("Recording metrics for command attempt: {CommandName}", commandName);
            activity?.AddEvent(new ActivityEvent("CommandAttempt"));

            var result = await next();

            // Analyze result for business success/failure
            var isSuccess = IsSuccessResult(result);
            var outcome = isSuccess ? "success" : "business_failure";

            tags.Add("Outcome", outcome);
            metricTags.Add("outcome", outcome);
            activity?.SetTag("command.outcome", outcome);
            activity?.SetStatus(isSuccess ? ActivityStatusCode.Ok : ActivityStatusCode.Error, outcome);

            // Record business metrics: outcome
            _metrics.RecordCounter($"{commandName}_{outcome}", tags);
            _metrics.RecordCounter($"command_{outcome}_total", tags);

            if (isSuccess)
            {
                CommandSuccess.Add(1, metricTags);
            }
            else
            {
                CommandFailures.Add(1, metricTags);
                activity?.AddEvent(new ActivityEvent("BusinessFailure"));
            }

            _logger.LogInformation("BUSINESS_METRIC: Command {CommandName} completed with outcome: {Outcome}",
                commandName, outcome);

            return result;
        }
        catch (Exception ex)
        {
            // Record exception in both systems
            tags.Add("Outcome", "exception");
            tags.Add("ExceptionType", ex.GetType().Name);

            metricTags.Add("outcome", "exception");
            metricTags.Add("exception.type", ex.GetType().Name);

            _metrics.RecordCounter($"{commandName}_exceptions", tags);
            _metrics.RecordCounter("command_exceptions_total", tags);
            CommandExceptions.Add(1, metricTags);

            // Record exception in activity
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);

            _logger.LogWarning("BUSINESS_METRIC: Command {CommandName} threw {ExceptionType}",
                commandName, ex.GetType().Name);

            throw;
        }
    }

    private static bool IsSuccessResult(TCommandResult result)
    {
        // Handle Result<T> pattern
        if (result.GetType().IsGenericType && result.GetType().GetGenericTypeDefinition() == typeof(Result<>))
        {
            var isSuccessProperty = result.GetType().GetProperty("IsSuccess");
            return (bool)(isSuccessProperty?.GetValue(result) ?? false);
        }

        // Handle other common success patterns
        if (result.GetType().Name.Contains("Result"))
        {
            var isSuccessProperty = result.GetType().GetProperty("IsSuccess") ??
                                   result.GetType().GetProperty("Success") ??
                                   result.GetType().GetProperty("Succeeded");
            if (isSuccessProperty != null)
            {
                return (bool)(isSuccessProperty.GetValue(result) ?? false);
            }
        }

        // Default: non-null result is considered success
        return result != null;
    }
}
