using Application.Behaviors;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Behaviors;

public class QueryPerformanceBehaviorTests
{
    private readonly Mock<ILogger<QueryPerformanceBehavior<TestQuery, TestResult>>> _loggerMock;

    public QueryPerformanceBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<QueryPerformanceBehavior<TestQuery, TestResult>>>();
    }

    [Fact]
    public async Task Handle_FastQuery_ReturnsResult()
    {
        // Arrange
        var behavior = new QueryPerformanceBehavior<TestQuery, TestResult>(
            _loggerMock.Object,
            TimeSpan.FromSeconds(3));

        var query = new TestQuery("test");
        var expectedResult = new TestResult(true);

        // Act
        var result = await behavior.Handle(query, CancellationToken.None, () => Task.FromResult(expectedResult));

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task Handle_QueryThatThrows_RethrowsException()
    {
        // Arrange
        var behavior = new QueryPerformanceBehavior<TestQuery, TestResult>(
            _loggerMock.Object);

        var query = new TestQuery("test");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(query, CancellationToken.None, () =>
                Task.FromException<TestResult>(new InvalidOperationException("Query failed"))));
    }

    [Fact]
    public async Task Handle_SlowQuery_StillReturnsResult()
    {
        // Arrange
        var behavior = new QueryPerformanceBehavior<TestQuery, TestResult>(
            _loggerMock.Object,
            TimeSpan.FromMilliseconds(1));

        var query = new TestQuery("test");
        var expectedResult = new TestResult(true);

        // Act
        var result = await behavior.Handle(query, CancellationToken.None, async () =>
        {
            await Task.Delay(10);
            return expectedResult;
        });

        // Assert
        Assert.Equal(expectedResult, result);
    }

    public record TestQuery(string Value);
    public record TestResult(bool IsSuccess);
}
