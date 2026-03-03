using Application.Behaviors;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Behaviors;

public class CommandPerformanceBehaviorTests
{
    private readonly Mock<ILogger<CommandPerformanceBehavior<TestCommand, TestResult>>> _loggerMock;

    public CommandPerformanceBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<CommandPerformanceBehavior<TestCommand, TestResult>>>();
    }

    [Fact]
    public async Task Handle_FastCommand_ReturnsResultWithoutSlowCommandWarning()
    {
        // Arrange
        var behavior = new CommandPerformanceBehavior<TestCommand, TestResult>(
            _loggerMock.Object,
            TimeSpan.FromSeconds(5));

        var command = new TestCommand("test");
        var expectedResult = new TestResult(true);

        // Act
        var result = await behavior.Handle(command, CancellationToken.None, () => Task.FromResult(expectedResult));

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task Handle_CommandThatThrows_RethrowsException()
    {
        // Arrange
        var behavior = new CommandPerformanceBehavior<TestCommand, TestResult>(
            _loggerMock.Object);

        var command = new TestCommand("test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(command, CancellationToken.None, () =>
                Task.FromException<TestResult>(new InvalidOperationException("Command failed"))));
    }

    [Fact]
    public async Task Handle_SlowCommand_StillReturnsResult()
    {
        // Arrange
        var behavior = new CommandPerformanceBehavior<TestCommand, TestResult>(
            _loggerMock.Object,
            TimeSpan.FromMilliseconds(1));

        var command = new TestCommand("test");
        var expectedResult = new TestResult(true);

        // Act
        var result = await behavior.Handle(command, CancellationToken.None, async () =>
        {
            await Task.Delay(10);
            return expectedResult;
        });

        // Assert
        Assert.Equal(expectedResult, result);
    }

    public record TestCommand(string Value);
    public record TestResult(bool IsSuccess);
}
