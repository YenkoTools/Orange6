using Application.Features.Users.Queries;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using Moq;

namespace Application.Tests.Features.Users.Queries;

public class GetUsersQueryHandlerTests
{
    private readonly Mock<IUserRepository> _repositoryMock;
    private readonly GetUsersQueryHandler _handler;

    public GetUsersQueryHandlerTests()
    {
        _repositoryMock = new Mock<IUserRepository>();
        _handler = new GetUsersQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_UsersExist_ReturnsSuccessResultWithPagedUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, Username = "user1" },
            new() { Id = 2, Username = "user2" }
        };
        var pagedResult = new PagedResult<User>(users, 2, 10, 1);
        var query = new GetUsersQuery(1, 10);

        _repositoryMock
            .Setup(r => r.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(2, result.Value.Items.Count());
    }

    [Fact]
    public async Task Handle_NoUsersExist_ReturnsFailureResult()
    {
        // Arrange
        var pagedResult = new PagedResult<User>([], 0, 10, 1);
        var query = new GetUsersQuery(1, 10);

        _repositoryMock
            .Setup(r => r.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsFailureResult()
    {
        // Arrange
        var query = new GetUsersQuery(1, 10);

        _repositoryMock
            .Setup(r => r.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_DefaultPageParameters_UsesDefaultValues()
    {
        // Arrange
        var query = new GetUsersQuery();
        var users = new List<User> { new() { Id = 1, Username = "user1" } };
        var pagedResult = new PagedResult<User>(users, 1, 10, 1);

        _repositoryMock
            .Setup(r => r.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _repositoryMock.Verify(r => r.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()), Times.Once);
    }
}
