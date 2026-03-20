using Application.Abstractions;
using Application.Behaviors;
using Domain.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Behaviors;

public class QueryMetricsBehaviorTests
{
    private readonly Mock<ILogger<QueryMetricsBehavior<TestQuery, TestResult>>> _loggerMock;
    private readonly Mock<IMetricsService> _metricsMock;

    public QueryMetricsBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<QueryMetricsBehavior<TestQuery, TestResult>>>();
        _metricsMock = new Mock<IMetricsService>();
    }

    [Fact]
    public async Task Handle_SuccessfulQuery_ReturnsResult()
    {
        // Arrange
        var behavior = new QueryMetricsBehavior<TestQuery, TestResult>(
            _loggerMock.Object,
            _metricsMock.Object);

        var query = new TestQuery("test");
        var expectedResult = new TestResult(true);

        // Act
        var result = await behavior.Handle(query, CancellationToken.None, () => Task.FromResult(expectedResult));

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task Handle_SuccessfulQuery_RecordsAttemptMetric()
    {
        // Arrange
        var behavior = new QueryMetricsBehavior<TestQuery, TestResult>(
            _loggerMock.Object,
            _metricsMock.Object);

        var query = new TestQuery("test");

        // Act
        await behavior.Handle(query, CancellationToken.None, () => Task.FromResult(new TestResult(true)));

        // Assert
        _metricsMock.Verify(m => m.RecordCounter(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_QueryThatThrows_RethrowsExceptionAndRecordsMetric()
    {
        // Arrange
        var behavior = new QueryMetricsBehavior<TestQuery, TestResult>(
            _loggerMock.Object,
            _metricsMock.Object);

        var query = new TestQuery("test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(query, CancellationToken.None, () =>
                Task.FromException<TestResult>(new InvalidOperationException("Query failed"))));

        _metricsMock.Verify(m => m.RecordCounter(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_SuccessResult_RecordsSuccessOutcome()
    {
        // Arrange
        var behavior = new QueryMetricsBehavior<TestQuery, Result<string>>(
            new Mock<ILogger<QueryMetricsBehavior<TestQuery, Result<string>>>>().Object,
            _metricsMock.Object);

        var query = new TestQuery("test");

        // Act
        await behavior.Handle(query, CancellationToken.None,
            () => Task.FromResult(Result<string>.Success("value")));

        // Assert
        _metricsMock.Verify(m => m.RecordCounter(
            It.Is<string>(s => s.Contains("success")),
            It.IsAny<Dictionary<string, string>>()),
            Times.AtLeastOnce);
    }

    public record TestQuery(string Value);
    public record TestResult(bool IsSuccess);
}
