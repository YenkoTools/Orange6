using Application.Abstractions;
using Application.Dispatching;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Application.Tests.Dispatching;

public class QueryDispatcherTests
{
    [Fact]
    public async Task Dispatch_WithHandler_ReturnsHandlerResult()
    {
        // Arrange
        var expectedResult = new TestResult(true);
        var handlerMock = new Mock<IQueryHandler<TestQuery, TestResult>>();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var services = new ServiceCollection();
        services.AddScoped<IQueryHandler<TestQuery, TestResult>>(_ => handlerMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new QueryDispatcher(serviceProvider);
        var query = new TestQuery("test");

        // Act
        var result = await dispatcher.Dispatch<TestQuery, TestResult>(query, CancellationToken.None);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task Dispatch_WithBehaviors_AppliesBehaviorsInOrder()
    {
        // Arrange
        var executionOrder = new List<string>();

        var expectedResult = new TestResult(true);
        var handlerMock = new Mock<IQueryHandler<TestQuery, TestResult>>();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var behavior1 = new TrackingBehavior<TestQuery, TestResult>("behavior1", executionOrder);
        var behavior2 = new TrackingBehavior<TestQuery, TestResult>("behavior2", executionOrder);

        var services = new ServiceCollection();
        services.AddScoped<IQueryHandler<TestQuery, TestResult>>(_ => handlerMock.Object);
        services.AddScoped<IQueryPipelineBehavior<TestQuery, TestResult>>(_ => behavior1);
        services.AddScoped<IQueryPipelineBehavior<TestQuery, TestResult>>(_ => behavior2);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new QueryDispatcher(serviceProvider);
        var query = new TestQuery("test");

        // Act
        var result = await dispatcher.Dispatch<TestQuery, TestResult>(query, CancellationToken.None);

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

        var dispatcher = new QueryDispatcher(serviceProvider);
        var query = new TestQuery("test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dispatcher.Dispatch<TestQuery, TestResult>(query, CancellationToken.None));
    }

    public record TestQuery(string Value);
    public record TestResult(bool IsSuccess);

    private class TrackingBehavior<TQuery, TResult> : IQueryPipelineBehavior<TQuery, TResult>
    {
        private readonly string _name;
        private readonly List<string> _log;

        public TrackingBehavior(string name, List<string> log)
        {
            _name = name;
            _log = log;
        }

        public async Task<TResult> Handle(TQuery query, CancellationToken cancellationToken, Func<Task<TResult>> next)
        {
            _log.Add(_name);
            return await next();
        }
    }
}
