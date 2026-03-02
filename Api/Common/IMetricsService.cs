namespace Api.Common;

/// <summary>
/// Service for recording business metrics and operational telemetry.
/// </summary>
public interface IMetricsService
{
    /// <summary>
    /// Records a counter metric (increments by 1).
    /// </summary>
    /// <param name="metricName">The name of the counter metric.</param>
    /// <param name="tags">Optional tags to add context to the metric.</param>
    void RecordCounter(string metricName, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Records a timer metric with duration.
    /// </summary>
    /// <param name="metricName">The name of the timer metric.</param>
    /// <param name="duration">The duration to record.</param>
    /// <param name="tags">Optional tags to add context to the metric.</param>
    void RecordTimer(string metricName, TimeSpan duration, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Records a gauge metric with a specific value.
    /// </summary>
    /// <param name="metricName">The name of the gauge metric.</param>
    /// <param name="value">The value to record.</param>
    /// <param name="tags">Optional tags to add context to the metric.</param>
    void RecordGauge(string metricName, double value, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Records a histogram metric with a specific value.
    /// </summary>
    /// <param name="metricName">The name of the histogram metric.</param>
    /// <param name="value">The value to record.</param>
    /// <param name="tags">Optional tags to add context to the metric.</param>
    void RecordHistogram(string metricName, double value, Dictionary<string, string>? tags = null);
}
