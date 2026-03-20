using Application.Features.Users.Commands;
using Application.Features.Users.Validation;

namespace Application.Tests.Features.Users.Validation;

public class UpdateUserCommandValidatorTests
{
    private readonly UpdateUserCommandValidator _validator;

    public UpdateUserCommandValidatorTests()
    {
        _validator = new UpdateUserCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ReturnsValidResult()
    {
        // Arrange
        var command = new UpdateUserCommand(1, "jdoe", "jdoe@example.com", "John", "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidId_ReturnsValidationError(int id)
    {
        // Arrange
        var command = new UpdateUserCommand(id, "jdoe", "jdoe@example.com", "John", "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateUserCommand.Id));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_EmptyUsername_ReturnsValidationError(string username)
    {
        // Arrange
        var command = new UpdateUserCommand(1, username, "jdoe@example.com", "John", "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateUserCommand.Username));
    }

    [Fact]
    public void Validate_UsernameTooLong_ReturnsValidationError()
    {
        // Arrange
        var command = new UpdateUserCommand(1, new string('a', 51), "jdoe@example.com", "John", "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateUserCommand.Username));
    }

    [Fact]
    public void Validate_InvalidEmailFormat_ReturnsValidationError()
    {
        // Arrange
        var command = new UpdateUserCommand(1, "jdoe", "not-an-email", "John", "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateUserCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_EmptyFirstName_ReturnsValidationError(string firstName)
    {
        // Arrange
        var command = new UpdateUserCommand(1, "jdoe", "jdoe@example.com", firstName, "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateUserCommand.FirstName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_EmptyLastName_ReturnsValidationError(string lastName)
    {
        // Arrange
        var command = new UpdateUserCommand(1, "jdoe", "jdoe@example.com", "John", lastName);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateUserCommand.LastName));
    }
}
