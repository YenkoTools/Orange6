using Application.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Behaviors;

public class QueryValidationBehaviorTests
{
    private readonly Mock<ILogger<QueryValidationBehavior<TestQuery, TestResult>>> _loggerMock;

    public QueryValidationBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<QueryValidationBehavior<TestQuery, TestResult>>>();
    }

    [Fact]
    public async Task Handle_NoValidators_CallsNextAndReturnsResult()
    {
        // Arrange
        var behavior = new QueryValidationBehavior<TestQuery, TestResult>(
            [],
            _loggerMock.Object);

        var query = new TestQuery("valid");
        var expectedResult = new TestResult(true);

        // Act
        var result = await behavior.Handle(query, CancellationToken.None, () => Task.FromResult(expectedResult));

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task Handle_ValidQuery_CallsNextAndReturnsResult()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestQuery>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestQuery>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new QueryValidationBehavior<TestQuery, TestResult>(
            [validatorMock.Object],
            _loggerMock.Object);

        var query = new TestQuery("valid");
        var expectedResult = new TestResult(true);

        // Act
        var result = await behavior.Handle(query, CancellationToken.None, () => Task.FromResult(expectedResult));

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task Handle_InvalidQuery_ThrowsValidationException()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("TestProperty", "Validation failed")
        };

        var validatorMock = new Mock<IValidator<TestQuery>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestQuery>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var behavior = new QueryValidationBehavior<TestQuery, TestResult>(
            [validatorMock.Object],
            _loggerMock.Object);

        var query = new TestQuery("invalid");

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(query, CancellationToken.None, () => Task.FromResult(new TestResult(true))));
    }

    public record TestQuery(string Value);
    public record TestResult(bool IsSuccess);
}
