using Application.Features.Users.Commands;
using Application.Interfaces;
using Domain.Entities;
using Domain.Errors;
using Moq;

namespace Application.Tests.Features.Users.Commands;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _handler = new CreateUserCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessResult()
    {
        // Arrange
        var command = new CreateUserCommand("jdoe", "jdoe@example.com", "John", "Doe");

        _repositoryMock
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("jdoe", result.Value.Username);
        Assert.Equal("jdoe@example.com", result.Value.Email);
        Assert.Equal("John", result.Value.FirstName);
        Assert.Equal("Doe", result.Value.LastName);
    }

    [Fact]
    public async Task Handle_ValidCommand_CallsRepositoryAddAsync()
    {
        // Arrange
        var command = new CreateUserCommand("jdoe", "jdoe@example.com", "John", "Doe");

        _repositoryMock
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateUsername_ReturnsFailureResult()
    {
        // Arrange
        var command = new CreateUserCommand("jdoe", "jdoe@example.com", "John", "Doe");
        var existingUser = new User { Username = command.Username };

        _repositoryMock
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.UserWithUsernameExists(command.Username).Code, result.Error.Code);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsCreatedAtAndUpdatedAt()
    {
        // Arrange
        var command = new CreateUserCommand("jdoe", "jdoe@example.com", "John", "Doe");
        var before = DateTime.UtcNow;

        _repositoryMock
            .Setup(r => r.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Value.CreatedAt >= before);
        Assert.True(result.Value.UpdatedAt >= before);
    }
}
