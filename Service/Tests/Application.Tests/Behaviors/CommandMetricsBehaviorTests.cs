using Application.Abstractions;
using Application.Behaviors;
using Domain.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Behaviors;

public class CommandMetricsBehaviorTests
{
    private readonly Mock<ILogger<CommandMetricsBehavior<TestCommand, TestResult>>> _loggerMock;
    private readonly Mock<IMetricsService> _metricsMock;

    public CommandMetricsBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<CommandMetricsBehavior<TestCommand, TestResult>>>();
        _metricsMock = new Mock<IMetricsService>();
    }

    [Fact]
    public async Task Handle_SuccessfulCommand_ReturnsResult()
    {
        // Arrange
        var behavior = new CommandMetricsBehavior<TestCommand, TestResult>(
            _loggerMock.Object,
            _metricsMock.Object);

        var command = new TestCommand("test");
        var expectedResult = new TestResult(true);

        // Act
        var result = await behavior.Handle(command, CancellationToken.None, () => Task.FromResult(expectedResult));

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task Handle_SuccessfulCommand_RecordsAttemptMetric()
    {
        // Arrange
        var behavior = new CommandMetricsBehavior<TestCommand, TestResult>(
            _loggerMock.Object,
            _metricsMock.Object);

        var command = new TestCommand("test");

        // Act
        await behavior.Handle(command, CancellationToken.None, () => Task.FromResult(new TestResult(true)));

        // Assert
        _metricsMock.Verify(m => m.RecordCounter(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_CommandThatThrows_RethrowsExceptionAndRecordsMetric()
    {
        // Arrange
        var behavior = new CommandMetricsBehavior<TestCommand, TestResult>(
            _loggerMock.Object,
            _metricsMock.Object);

        var command = new TestCommand("test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(command, CancellationToken.None, () =>
                Task.FromException<TestResult>(new InvalidOperationException("Command failed"))));

        _metricsMock.Verify(m => m.RecordCounter(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_SuccessResult_RecordsSuccessOutcome()
    {
        // Arrange
        var behavior = new CommandMetricsBehavior<TestCommand, Result<string>>(
            new Mock<ILogger<CommandMetricsBehavior<TestCommand, Result<string>>>>().Object,
            _metricsMock.Object);

        var command = new TestCommand("test");

        // Act
        await behavior.Handle(command, CancellationToken.None,
            () => Task.FromResult(Result<string>.Success("value")));

        // Assert
        _metricsMock.Verify(m => m.RecordCounter(
            It.Is<string>(s => s.Contains("success")),
            It.IsAny<Dictionary<string, string>>()),
            Times.AtLeastOnce);
    }

    public record TestCommand(string Value);
    public record TestResult(bool IsSuccess);
}
