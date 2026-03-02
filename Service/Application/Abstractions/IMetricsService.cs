namespace Orange6.Application.Abstractions;

/// <summary>
/// Service for recording business metrics and operational telemetry.
/// </summary>
public interface IMetricsService
{
    /// <summary>Records a counter metric (increments by 1).</summary>
    void RecordCounter(string metricName, Dictionary<string, string>? tags = null);

    /// <summary>Records a timer metric with duration.</summary>
    void RecordTimer(string metricName, TimeSpan duration, Dictionary<string, string>? tags = null);

    /// <summary>Records a gauge metric with a specific value.</summary>
    void RecordGauge(string metricName, double value, Dictionary<string, string>? tags = null);

    /// <summary>Records a histogram metric with a specific value.</summary>
    void RecordHistogram(string metricName, double value, Dictionary<string, string>? tags = null);
}
