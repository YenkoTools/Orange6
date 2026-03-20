using Application.Features.Users.Commands;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using Moq;

namespace Application.Tests.Features.Users.Commands;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _handler = new DeleteUserCommandHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingUser_ReturnsSuccessResult()
    {
        // Arrange
        var user = new User { Id = 1, Username = "jdoe" };
        var command = new DeleteUserCommand(1);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ExistingUser_CallsRepositoryDeleteAsync()
    {
        // Arrange
        var user = new User { Id = 1, Username = "jdoe" };
        var command = new DeleteUserCommand(1);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingUser_ReturnsNotFoundResult()
    {
        // Arrange
        var command = new DeleteUserCommand(99);

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
    public async Task Handle_NonExistingUser_DoesNotCallDeleteAsync()
    {
        // Arrange
        var command = new DeleteUserCommand(99);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
