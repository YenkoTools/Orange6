using Application.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Behaviors;

public class CommandValidationBehaviorTests
{
    private readonly Mock<ILogger<CommandValidationBehavior<TestCommand, TestResult>>> _loggerMock;

    public CommandValidationBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<CommandValidationBehavior<TestCommand, TestResult>>>();
    }

    [Fact]
    public async Task Handle_NoValidators_CallsNextAndReturnsResult()
    {
        // Arrange
        var behavior = new CommandValidationBehavior<TestCommand, TestResult>(
            [],
            _loggerMock.Object);

        var command = new TestCommand("valid");
        var expectedResult = new TestResult(true);

        // Act
        var result = await behavior.Handle(command, CancellationToken.None, () => Task.FromResult(expectedResult));

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsNextAndReturnsResult()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new CommandValidationBehavior<TestCommand, TestResult>(
            [validatorMock.Object],
            _loggerMock.Object);

        var command = new TestCommand("valid");
        var expectedResult = new TestResult(true);

        // Act
        var result = await behavior.Handle(command, CancellationToken.None, () => Task.FromResult(expectedResult));

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        // Arrange
        var failures = new List<ValidationFailure>
        {
            new("TestProperty", "Validation failed")
        };

        var validatorMock = new Mock<IValidator<TestCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var behavior = new CommandValidationBehavior<TestCommand, TestResult>(
            [validatorMock.Object],
            _loggerMock.Object);

        var command = new TestCommand("invalid");

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(command, CancellationToken.None, () => Task.FromResult(new TestResult(true))));
    }

    public record TestCommand(string Value);
    public record TestResult(bool IsSuccess);
}
