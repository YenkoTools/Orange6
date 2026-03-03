using Domain.Entities;
using Infrastructure.Repositories;

namespace Infrastructure.Tests.Repositories;

public class InMemoryUserRepositoryTests
{
    private static User CreateUser(string username = "testuser", string email = "test@example.com") =>
        new() { Username = username, Email = email, FirstName = "Test", LastName = "User" };

    // AddAsync

    [Fact]
    public async Task AddAsync_ShouldAssignIncrementalId()
    {
        var repo = new InMemoryUserRepository();
        var user1 = await repo.AddAsync(CreateUser("u1", "u1@example.com"));
        var user2 = await repo.AddAsync(CreateUser("u2", "u2@example.com"));

        Assert.Equal(1, user1.Id);
        Assert.Equal(2, user2.Id);
    }

    [Fact]
    public async Task AddAsync_ShouldSetCreatedAt()
    {
        var before = DateTime.UtcNow;
        var repo = new InMemoryUserRepository();
        var user = await repo.AddAsync(CreateUser());

        Assert.True(user.CreatedAt >= before);
    }

    [Fact]
    public async Task AddAsync_ShouldSetUpdatedAt()
    {
        var before = DateTime.UtcNow;
        var repo = new InMemoryUserRepository();
        var user = await repo.AddAsync(CreateUser());

        Assert.True(user.UpdatedAt >= before);
    }

    [Fact]
    public async Task AddAsync_ShouldStoreUser()
    {
        var repo = new InMemoryUserRepository();
        var user = CreateUser();
        await repo.AddAsync(user);

        var result = await repo.GetByIdAsync(user.Id);
        Assert.NotNull(result);
        Assert.Equal(user.Username, result.Username);
    }

    // GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
    {
        var repo = new InMemoryUserRepository();
        var added = await repo.AddAsync(CreateUser());

        var result = await repo.GetByIdAsync(added.Id);

        Assert.NotNull(result);
        Assert.Equal(added.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var repo = new InMemoryUserRepository();

        var result = await repo.GetByIdAsync(999);

        Assert.Null(result);
    }

    // GetAllAsync

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        var repo = new InMemoryUserRepository();
        await repo.AddAsync(CreateUser("u1", "u1@example.com"));
        await repo.AddAsync(CreateUser("u2", "u2@example.com"));

        var result = await repo.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoUsers()
    {
        var repo = new InMemoryUserRepository();

        var result = await repo.GetAllAsync();

        Assert.Empty(result);
    }

    // FindAsync

    [Fact]
    public async Task FindAsync_ShouldReturnMatchingUsers()
    {
        var repo = new InMemoryUserRepository();
        await repo.AddAsync(CreateUser("alice", "alice@example.com"));
        await repo.AddAsync(CreateUser("bob", "bob@example.com"));

        var result = await repo.FindAsync(u => u.Username == "alice");

        Assert.Single(result);
        Assert.Equal("alice", result.First().Username);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnEmpty_WhenNoMatch()
    {
        var repo = new InMemoryUserRepository();
        await repo.AddAsync(CreateUser());

        var result = await repo.FindAsync(u => u.Username == "nonexistent");

        Assert.Empty(result);
    }

    // GetPagedAsync

    [Fact]
    public async Task GetPagedAsync_ShouldReturnCorrectItems()
    {
        var repo = new InMemoryUserRepository();
        for (int i = 1; i <= 5; i++)
            await repo.AddAsync(CreateUser($"user{i}", $"user{i}@example.com"));

        var result = await repo.GetPagedAsync(page: 1, pageSize: 3);

        Assert.Equal(3, result.Items.Count());
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnCorrectTotalCount()
    {
        var repo = new InMemoryUserRepository();
        for (int i = 1; i <= 5; i++)
            await repo.AddAsync(CreateUser($"user{i}", $"user{i}@example.com"));

        var result = await repo.GetPagedAsync(page: 1, pageSize: 3);

        Assert.Equal(5, result.TotalCount);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnSecondPage()
    {
        var repo = new InMemoryUserRepository();
        for (int i = 1; i <= 5; i++)
            await repo.AddAsync(CreateUser($"user{i}", $"user{i}@example.com"));

        var result = await repo.GetPagedAsync(page: 2, pageSize: 3);

        Assert.Equal(2, result.Items.Count());
        Assert.Equal(2, result.PageNumber);
    }

    // UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        var repo = new InMemoryUserRepository();
        var user = await repo.AddAsync(CreateUser());
        user.FirstName = "Updated";

        await repo.UpdateAsync(user);

        var result = await repo.GetByIdAsync(user.Id);
        Assert.NotNull(result);
        Assert.Equal("Updated", result.FirstName);
    }

    [Fact]
    public async Task UpdateAsync_ShouldSetUpdatedAt()
    {
        var repo = new InMemoryUserRepository();
        var user = await repo.AddAsync(CreateUser());
        var before = DateTime.UtcNow;
        user.FirstName = "Modified";

        await repo.UpdateAsync(user);

        var result = await repo.GetByIdAsync(user.Id);
        Assert.NotNull(result);
        Assert.True(result.UpdatedAt >= before);
    }

    [Fact]
    public async Task UpdateAsync_ShouldDoNothing_WhenUserNotFound()
    {
        var repo = new InMemoryUserRepository();
        var nonExistentUser = new User { Id = 999, Username = "ghost", Email = "ghost@example.com" };

        var exception = await Record.ExceptionAsync(() => repo.UpdateAsync(nonExistentUser));

        Assert.Null(exception);
    }

    // DeleteAsync

    [Fact]
    public async Task DeleteAsync_ShouldRemoveUser()
    {
        var repo = new InMemoryUserRepository();
        var user = await repo.AddAsync(CreateUser());

        await repo.DeleteAsync(user);

        var result = await repo.GetByIdAsync(user.Id);
        Assert.Null(result);
    }

    // DeleteByIdAsync

    [Fact]
    public async Task DeleteByIdAsync_ShouldRemoveUser_WhenExists()
    {
        var repo = new InMemoryUserRepository();
        var user = await repo.AddAsync(CreateUser());

        await repo.DeleteByIdAsync(user.Id);

        var result = await repo.GetByIdAsync(user.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldNotThrow_WhenUserNotFound()
    {
        var repo = new InMemoryUserRepository();

        var exception = await Record.ExceptionAsync(() => repo.DeleteByIdAsync(999));

        Assert.Null(exception);
    }

    // CountAsync

    [Fact]
    public async Task CountAsync_ShouldReturnZero_WhenEmpty()
    {
        var repo = new InMemoryUserRepository();

        var count = await repo.CountAsync();

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        var repo = new InMemoryUserRepository();
        await repo.AddAsync(CreateUser("u1", "u1@example.com"));
        await repo.AddAsync(CreateUser("u2", "u2@example.com"));

        var count = await repo.CountAsync();

        Assert.Equal(2, count);
    }

    // AnyAsync

    [Fact]
    public async Task AnyAsync_ShouldReturnTrue_WhenMatchExists()
    {
        var repo = new InMemoryUserRepository();
        await repo.AddAsync(CreateUser("alice", "alice@example.com"));

        var result = await repo.AnyAsync(u => u.Username == "alice");

        Assert.True(result);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnFalse_WhenNoMatch()
    {
        var repo = new InMemoryUserRepository();
        await repo.AddAsync(CreateUser());

        var result = await repo.AnyAsync(u => u.Username == "nonexistent");

        Assert.False(result);
    }

    // GetByUsernameAsync

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnUser_WhenExists()
    {
        var repo = new InMemoryUserRepository();
        await repo.AddAsync(CreateUser("alice", "alice@example.com"));

        var result = await repo.GetByUsernameAsync("alice");

        Assert.NotNull(result);
        Assert.Equal("alice", result.Username);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnNull_WhenNotFound()
    {
        var repo = new InMemoryUserRepository();

        var result = await repo.GetByUsernameAsync("nonexistent");

        Assert.Null(result);
    }

    // GetByEmailAsync

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenExists()
    {
        var repo = new InMemoryUserRepository();
        await repo.AddAsync(CreateUser("alice", "alice@example.com"));

        var result = await repo.GetByEmailAsync("alice@example.com");

        Assert.NotNull(result);
        Assert.Equal("alice@example.com", result.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenNotFound()
    {
        var repo = new InMemoryUserRepository();

        var result = await repo.GetByEmailAsync("nobody@example.com");

        Assert.Null(result);
    }
}
