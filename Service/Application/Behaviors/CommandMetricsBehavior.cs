using Microsoft.Extensions.Logging;
using Orange6.Application.Abstractions;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Orange6.Application.Behaviors;

/// <summary>
/// Pipeline behavior that collects business metrics for commands.
/// </summary>
public class CommandMetricsBehavior<TCommand, TCommandResult> : ICommandPipelineBehavior<TCommand, TCommandResult>
    where TCommandResult : class
{
    private readonly ILogger<CommandMetricsBehavior<TCommand, TCommandResult>> _logger;
    private readonly IMetricsService _metrics;

    private static readonly ActivitySource ActivitySource = new("Orange6.Application.Commands");
    private static readonly Meter Meter = new("Orange6.Application.Commands");
    private static readonly Counter<long> CommandAttempts = Meter.CreateCounter<long>(
        "orange6.command.attempts", "attempts", "Number of command attempts");
    private static readonly Counter<long> CommandSuccess = Meter.CreateCounter<long>(
        "orange6.command.success", "successes", "Number of successful commands");
    private static readonly Counter<long> CommandFailures = Meter.CreateCounter<long>(
        "orange6.command.failures", "failures", "Number of failed commands");
    private static readonly Counter<long> CommandExceptions = Meter.CreateCounter<long>(
        "orange6.command.exceptions", "exceptions", "Number of command exceptions");

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

        using var activity = ActivitySource.StartActivity($"Command.{commandName}", ActivityKind.Internal);
        activity?.SetTag("command.type", commandName);
        activity?.SetTag("command.fullname", typeof(TCommand).FullName);
        activity?.SetTag("operation", "command");

        var tags = new Dictionary<string, string>
        {
            ["CommandType"] = commandName,
            ["Operation"] = "command"
        };

        var metricTags = new TagList
        {
            { "command.type", commandName },
            { "operation", "command" }
        };

        try
        {
            _metrics.RecordCounter($"{commandName}_attempts", tags);
            _metrics.RecordCounter("command_attempts_total", tags);
            CommandAttempts.Add(1, metricTags);

            activity?.AddEvent(new ActivityEvent("CommandAttempt"));

            var result = await next();

            var isSuccess = IsSuccessResult(result);
            var outcome = isSuccess ? "success" : "business_failure";

            tags.Add("Outcome", outcome);
            metricTags.Add("outcome", outcome);
            activity?.SetTag("command.outcome", outcome);
            activity?.SetStatus(isSuccess ? ActivityStatusCode.Ok : ActivityStatusCode.Error, outcome);

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

            _logger.LogInformation("BUSINESS_METRIC: Command {CommandName} completed with outcome: {Outcome}", commandName, outcome);

            return result;
        }
        catch (Exception ex)
        {
            tags.Add("Outcome", "exception");
            tags.Add("ExceptionType", ex.GetType().Name);
            metricTags.Add("outcome", "exception");
            metricTags.Add("exception.type", ex.GetType().Name);

            _metrics.RecordCounter($"{commandName}_exceptions", tags);
            _metrics.RecordCounter("command_exceptions_total", tags);
            CommandExceptions.Add(1, metricTags);

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection
            {
                { "exception.type", ex.GetType().FullName },
                { "exception.message", ex.Message }
            }));

            _logger.LogWarning("BUSINESS_METRIC: Command {CommandName} threw {ExceptionType}", commandName, ex.GetType().Name);

            throw;
        }
    }

    private static bool IsSuccessResult(TCommandResult result)
    {
        var type = result.GetType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Orange6.Domain.Common.Result<>))
        {
            var prop = type.GetProperty("IsSuccess");
            return (bool)(prop?.GetValue(result) ?? false);
        }

        if (type.Name.Contains("Result"))
        {
            var prop = type.GetProperty("IsSuccess") ??
                       type.GetProperty("Success") ??
                       type.GetProperty("Succeeded");
            if (prop != null)
                return (bool)(prop.GetValue(result) ?? false);
        }

        return result != null;
    }
}
