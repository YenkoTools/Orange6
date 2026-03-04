using Application.Features.Users.Commands;
using Application.Interfaces;
using Domain.Entities;
using Moq;

namespace Application.Tests.Features.Users.Commands;

public class UpdateUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _handler = new UpdateUserCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingUser_ReturnsSuccessResultWithUpdatedUser()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            Username = "old_username",
            Email = "old@example.com",
            FirstName = "Old",
            LastName = "Name"
        };

        var command = new UpdateUserCommand(1, "new_username", "new@example.com", "New", "Name");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("new_username", result.Value.Username);
        Assert.Equal("new@example.com", result.Value.Email);
        Assert.Equal("New", result.Value.FirstName);
        Assert.Equal("Name", result.Value.LastName);
    }

    [Fact]
    public async Task Handle_ExistingUser_CallsRepositoryUpdateAsync()
    {
        // Arrange
        var existingUser = new User { Id = 1, Username = "jdoe" };
        var command = new UpdateUserCommand(1, "jdoe_updated", "jdoe@example.com", "John", "Doe");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.UpdateAsync(existingUser, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingUser_ReturnsNotFoundResult()
    {
        // Arrange
        var command = new UpdateUserCommand(99, "jdoe", "jdoe@example.com", "John", "Doe");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsNotFound);
    }

    [Fact]
    public async Task Handle_NonExistingUser_DoesNotCallUpdateAsync()
    {
        // Arrange
        var command = new UpdateUserCommand(99, "jdoe", "jdoe@example.com", "John", "Doe");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
