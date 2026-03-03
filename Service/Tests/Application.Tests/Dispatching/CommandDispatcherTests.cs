using Application.Abstractions;
using Application.Dispatching;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Application.Tests.Dispatching;

public class CommandDispatcherTests
{
    [Fact]
    public async Task Dispatch_WithHandler_ReturnsHandlerResult()
    {
        // Arrange
        var expectedResult = new TestResult(true);
        var handlerMock = new Mock<ICommandHandler<TestCommand, TestResult>>();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<TestCommand, TestResult>>(_ => handlerMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new CommandDispatcher(serviceProvider);
        var command = new TestCommand("test");

        // Act
        var result = await dispatcher.Dispatch<TestCommand, TestResult>(command, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task Dispatch_WithBehaviors_AppliesBehaviorsInOrder()
    {
        // Arrange
        var executionOrder = new List<string>();

        var expectedResult = new TestResult(true);
        var handlerMock = new Mock<ICommandHandler<TestCommand, TestResult>>();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var behavior1 = new TrackingBehavior<TestCommand, TestResult>("behavior1", executionOrder);
        var behavior2 = new TrackingBehavior<TestCommand, TestResult>("behavior2", executionOrder);

        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<TestCommand, TestResult>>(_ => handlerMock.Object);
        services.AddScoped<ICommandPipelineBehavior<TestCommand, TestResult>>(_ => behavior1);
        services.AddScoped<ICommandPipelineBehavior<TestCommand, TestResult>>(_ => behavior2);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new CommandDispatcher(serviceProvider);
        var command = new TestCommand("test");

        // Act
        var result = await dispatcher.Dispatch<TestCommand, TestResult>(command, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResult, result);
        Assert.Equal(2, executionOrder.Count);
    }

    [Fact]
    public async Task Dispatch_MissingHandler_ThrowsException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new CommandDispatcher(serviceProvider);
        var command = new TestCommand("test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dispatcher.Dispatch<TestCommand, TestResult>(command, CancellationToken.None));
    }

    public record TestCommand(string Value);
    public record TestResult(bool IsSuccess);

    private class TrackingBehavior<TCommand, TResult> : ICommandPipelineBehavior<TCommand, TResult>
    {
        private readonly string _name;
        private readonly List<string> _log;

        public TrackingBehavior(string name, List<string> log)
        {
            _name = name;
            _log = log;
        }

        public async Task<TResult> Handle(TCommand command, CancellationToken cancellationToken, Func<Task<TResult>> next)
        {
            _log.Add(_name);
            return await next();
        }
    }
}
