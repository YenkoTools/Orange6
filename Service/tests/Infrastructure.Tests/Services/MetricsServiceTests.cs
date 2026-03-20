using Microsoft.Extensions.Logging.Abstractions;
using Infrastructure.Services;

namespace Infrastructure.Tests.Services;

public class MetricsServiceTests
{
    private static MetricsService CreateService() =>
        new(NullLogger<MetricsService>.Instance);

    // RecordCounter

    [Fact]
    public void RecordCounter_ShouldNotThrow()
    {
        var service = CreateService();

        var exception = Record.Exception(() => service.RecordCounter("test.counter"));

        Assert.Null(exception);
    }

    [Fact]
    public void RecordCounter_WithTags_ShouldNotThrow()
    {
        var service = CreateService();
        var tags = new Dictionary<string, string> { ["env"] = "test", ["region"] = "us-east" };

        var exception = Record.Exception(() => service.RecordCounter("test.counter", tags));

        Assert.Null(exception);
    }

    [Fact]
    public void RecordCounter_WithNullTags_ShouldNotThrow()
    {
        var service = CreateService();

        var exception = Record.Exception(() => service.RecordCounter("test.counter", null));

        Assert.Null(exception);
    }

    // RecordTimer

    [Fact]
    public void RecordTimer_ShouldNotThrow()
    {
        var service = CreateService();

        var exception = Record.Exception(() => service.RecordTimer("test.timer", TimeSpan.FromMilliseconds(150)));

        Assert.Null(exception);
    }

    [Fact]
    public void RecordTimer_WithTags_ShouldNotThrow()
    {
        var service = CreateService();
        var tags = new Dictionary<string, string> { ["operation"] = "query" };

        var exception = Record.Exception(() => service.RecordTimer("test.timer", TimeSpan.FromSeconds(1), tags));

        Assert.Null(exception);
    }

    [Fact]
    public void RecordTimer_WithNullTags_ShouldNotThrow()
    {
        var service = CreateService();

        var exception = Record.Exception(() => service.RecordTimer("test.timer", TimeSpan.Zero, null));

        Assert.Null(exception);
    }

    // RecordGauge

    [Fact]
    public void RecordGauge_ShouldNotThrow()
    {
        var service = CreateService();

        var exception = Record.Exception(() => service.RecordGauge("test.gauge", 42.5));

        Assert.Null(exception);
    }

    [Fact]
    public void RecordGauge_WithTags_ShouldNotThrow()
    {
        var service = CreateService();
        var tags = new Dictionary<string, string> { ["unit"] = "bytes" };

        var exception = Record.Exception(() => service.RecordGauge("test.gauge", 1024.0, tags));

        Assert.Null(exception);
    }

    [Fact]
    public void RecordGauge_WithNullTags_ShouldNotThrow()
    {
        var service = CreateService();

        var exception = Record.Exception(() => service.RecordGauge("test.gauge", 0.0, null));

        Assert.Null(exception);
    }

    // RecordHistogram

    [Fact]
    public void RecordHistogram_ShouldNotThrow()
    {
        var service = CreateService();

        var exception = Record.Exception(() => service.RecordHistogram("test.histogram", 99.9));

        Assert.Null(exception);
    }

    [Fact]
    public void RecordHistogram_WithTags_ShouldNotThrow()
    {
        var service = CreateService();
        var tags = new Dictionary<string, string> { ["percentile"] = "p99" };

        var exception = Record.Exception(() => service.RecordHistogram("test.histogram", 250.0, tags));

        Assert.Null(exception);
    }

    [Fact]
    public void RecordHistogram_WithNullTags_ShouldNotThrow()
    {
        var service = CreateService();

        var exception = Record.Exception(() => service.RecordHistogram("test.histogram", 0.0, null));

        Assert.Null(exception);
    }
}
