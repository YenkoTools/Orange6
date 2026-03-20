using Application.Features.Users.Commands;
using Application.Features.Users.Validation;

namespace Application.Tests.Features.Users.Validation;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator;

    public CreateUserCommandValidatorTests()
    {
        _validator = new CreateUserCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ReturnsValidResult()
    {
        // Arrange
        var command = new CreateUserCommand("jdoe", "jdoe@example.com", "John", "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_EmptyUsername_ReturnsValidationError(string username)
    {
        // Arrange
        var command = new CreateUserCommand(username, "jdoe@example.com", "John", "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.Username));
    }

    [Fact]
    public void Validate_UsernameTooLong_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateUserCommand(new string('a', 51), "jdoe@example.com", "John", "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.Username));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_EmptyEmail_ReturnsValidationError(string email)
    {
        // Arrange
        var command = new CreateUserCommand("jdoe", email, "John", "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.Email));
    }

    [Fact]
    public void Validate_InvalidEmailFormat_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateUserCommand("jdoe", "not-an-email", "John", "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.Email));
    }

    [Fact]
    public void Validate_EmailTooLong_ReturnsValidationError()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@x.com";
        var command = new CreateUserCommand("jdoe", longEmail, "John", "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_EmptyFirstName_ReturnsValidationError(string firstName)
    {
        // Arrange
        var command = new CreateUserCommand("jdoe", "jdoe@example.com", firstName, "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.FirstName));
    }

    [Fact]
    public void Validate_FirstNameTooLong_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateUserCommand("jdoe", "jdoe@example.com", new string('a', 101), "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.FirstName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_EmptyLastName_ReturnsValidationError(string lastName)
    {
        // Arrange
        var command = new CreateUserCommand("jdoe", "jdoe@example.com", "John", lastName);

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.LastName));
    }

    [Fact]
    public void Validate_LastNameTooLong_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateUserCommand("jdoe", "jdoe@example.com", "John", new string('a', 101));

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.LastName));
    }
}
