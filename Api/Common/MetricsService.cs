using Microsoft.Extensions.Logging;

namespace Api.Common;

/// <summary>
/// Default implementation of IMetricsService that logs metrics.
/// Can be replaced with a more sophisticated implementation (e.g., OpenTelemetry, Prometheus).
/// </summary>
public class MetricsService : IMetricsService
{
    private readonly ILogger<MetricsService> _logger;

    public MetricsService(ILogger<MetricsService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void RecordCounter(string metricName, Dictionary<string, string>? tags = null)
    {
        var tagsString = FormatTags(tags);
        _logger.LogDebug("Metric [Counter] {MetricName}{Tags}", metricName, tagsString);
    }

    /// <inheritdoc />
    public void RecordTimer(string metricName, TimeSpan duration, Dictionary<string, string>? tags = null)
    {
        var tagsString = FormatTags(tags);
        _logger.LogDebug("Metric [Timer] {MetricName} = {Duration}ms{Tags}", 
            metricName, duration.TotalMilliseconds, tagsString);
    }

    /// <inheritdoc />
    public void RecordGauge(string metricName, double value, Dictionary<string, string>? tags = null)
    {
        var tagsString = FormatTags(tags);
        _logger.LogDebug("Metric [Gauge] {MetricName} = {Value}{Tags}", 
            metricName, value, tagsString);
    }

    /// <inheritdoc />
    public void RecordHistogram(string metricName, double value, Dictionary<string, string>? tags = null)
    {
        var tagsString = FormatTags(tags);
        _logger.LogDebug("Metric [Histogram] {MetricName} = {Value}{Tags}", 
            metricName, value, tagsString);
    }

    private static string FormatTags(Dictionary<string, string>? tags)
    {
        if (tags == null || tags.Count == 0)
            return string.Empty;

        var formattedTags = string.Join(", ", tags.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return $" [{formattedTags}]";
    }
}
