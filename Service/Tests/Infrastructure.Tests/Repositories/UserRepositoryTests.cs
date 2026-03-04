using Domain.Entities;
using Infrastructure.Configuration;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Infrastructure.Tests.Repositories;

/// <summary>
/// Integration tests for UserRepository using a temporary file-based SQLite database.
/// </summary>
public class UserRepositoryTests : IDisposable
{
    private readonly string _dbPath;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"orange6_test_{Guid.NewGuid():N}");

        var options = Options.Create(new DatabaseOptions { DatabasePath = _dbPath });
        var factory = new DatabaseFactory(options);
        var initializer = new DatabaseInitializer(factory, NullLogger<DatabaseInitializer>.Instance);
        initializer.Initialize();

        _repository = new UserRepository(factory);
    }

    public void Dispose()
    {
        var dbFile = $"{_dbPath}.sqlite";
        if (File.Exists(dbFile))
            File.Delete(dbFile);
    }

    private const int SeedCount = 35;

    private static User CreateUser(string username = "testuser", string email = "test@example.com") =>
        new() { Username = username, Email = email, FirstName = "Test", LastName = "User" };

    // AddAsync

    [Fact]
    public async Task AddAsync_ShouldAssignId()
    {
        var user = await _repository.AddAsync(CreateUser());

        Assert.True(user.Id > 0);
    }

    [Fact]
    public async Task AddAsync_ShouldAssignIncrementalId()
    {
        var user1 = await _repository.AddAsync(CreateUser("u1", "u1@example.com"));
        var user2 = await _repository.AddAsync(CreateUser("u2", "u2@example.com"));

        Assert.True(user2.Id > user1.Id);
    }

    [Fact]
    public async Task AddAsync_ShouldSetCreatedAt()
    {
        var before = DateTime.UtcNow;
        var user = await _repository.AddAsync(CreateUser());

        Assert.True(user.CreatedAt >= before);
    }

    [Fact]
    public async Task AddAsync_ShouldSetUpdatedAt()
    {
        var before = DateTime.UtcNow;
        var user = await _repository.AddAsync(CreateUser());

        Assert.True(user.UpdatedAt >= before);
    }

    [Fact]
    public async Task AddAsync_ShouldStoreUser()
    {
        var user = CreateUser();
        await _repository.AddAsync(user);

        var result = await _repository.GetByIdAsync(user.Id);
        Assert.NotNull(result);
        Assert.Equal(user.Username, result.Username);
    }

    // GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
    {
        var added = await _repository.AddAsync(CreateUser());

        var result = await _repository.GetByIdAsync(added.Id);

        Assert.NotNull(result);
        Assert.Equal(added.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    // GetAllAsync

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllUsers()
    {
        await _repository.AddAsync(CreateUser("u1", "u1@example.com"));
        await _repository.AddAsync(CreateUser("u2", "u2@example.com"));

        var result = await _repository.GetAllAsync();

        Assert.Equal(SeedCount + 2, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSeedUsers_WhenNoUsersAdded()
    {
        var result = await _repository.GetAllAsync();

        Assert.Equal(SeedCount, result.Count());
    }

    // FindAsync

    [Fact]
    public async Task FindAsync_ShouldReturnMatchingUsers()
    {
        await _repository.AddAsync(CreateUser("alice", "alice@example.com"));
        await _repository.AddAsync(CreateUser("bob", "bob@example.com"));

        var result = await _repository.FindAsync(u => u.Username == "alice");

        Assert.Single(result);
        Assert.Equal("alice", result.First().Username);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnEmpty_WhenNoMatch()
    {
        await _repository.AddAsync(CreateUser());

        var result = await _repository.FindAsync(u => u.Username == "nonexistent");

        Assert.Empty(result);
    }

    // GetPagedAsync

    [Fact]
    public async Task GetPagedAsync_ShouldReturnCorrectItems()
    {
        for (int i = 1; i <= 5; i++)
            await _repository.AddAsync(CreateUser($"user{i}", $"user{i}@example.com"));

        var result = await _repository.GetPagedAsync(page: 1, pageSize: 3);

        Assert.Equal(3, result.Items.Count());
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnCorrectTotalCount()
    {
        for (int i = 1; i <= 5; i++)
            await _repository.AddAsync(CreateUser($"user{i}", $"user{i}@example.com"));

        var result = await _repository.GetPagedAsync(page: 1, pageSize: 3);

        Assert.Equal(SeedCount + 5, result.TotalCount);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnSecondPage()
    {
        for (int i = 1; i <= 5; i++)
            await _repository.AddAsync(CreateUser($"user{i}", $"user{i}@example.com"));

        var result = await _repository.GetPagedAsync(page: 2, pageSize: 3);

        Assert.Equal(3, result.Items.Count());
        Assert.Equal(2, result.PageNumber);
    }

    // UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser()
    {
        var user = await _repository.AddAsync(CreateUser());
        user.FirstName = "Updated";

        await _repository.UpdateAsync(user);

        var result = await _repository.GetByIdAsync(user.Id);
        Assert.NotNull(result);
        Assert.Equal("Updated", result.FirstName);
    }

    [Fact]
    public async Task UpdateAsync_ShouldSetUpdatedAt()
    {
        var user = await _repository.AddAsync(CreateUser());
        var before = DateTime.UtcNow;
        user.FirstName = "Modified";

        await _repository.UpdateAsync(user);

        var result = await _repository.GetByIdAsync(user.Id);
        Assert.NotNull(result);
        Assert.True(result.UpdatedAt >= before);
    }

    [Fact]
    public async Task UpdateAsync_ShouldDoNothing_WhenUserNotFound()
    {
        var nonExistentUser = new User { Id = 999, Username = "ghost", Email = "ghost@example.com", FirstName = "Ghost", LastName = "User" };

        var exception = await Record.ExceptionAsync(() => _repository.UpdateAsync(nonExistentUser));

        Assert.Null(exception);
    }

    // DeleteAsync

    [Fact]
    public async Task DeleteAsync_ShouldRemoveUser()
    {
        var user = await _repository.AddAsync(CreateUser());

        await _repository.DeleteAsync(user);

        var result = await _repository.GetByIdAsync(user.Id);
        Assert.Null(result);
    }

    // DeleteByIdAsync

    [Fact]
    public async Task DeleteByIdAsync_ShouldRemoveUser_WhenExists()
    {
        var user = await _repository.AddAsync(CreateUser());

        await _repository.DeleteByIdAsync(user.Id);

        var result = await _repository.GetByIdAsync(user.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldNotThrow_WhenUserNotFound()
    {
        var exception = await Record.ExceptionAsync(() => _repository.DeleteByIdAsync(999));

        Assert.Null(exception);
    }

    // CountAsync

    [Fact]
    public async Task CountAsync_ShouldReturnSeedCount_WhenNoUsersAdded()
    {
        var count = await _repository.CountAsync();

        Assert.Equal(SeedCount, count);
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        await _repository.AddAsync(CreateUser("u1", "u1@example.com"));
        await _repository.AddAsync(CreateUser("u2", "u2@example.com"));

        var count = await _repository.CountAsync();

        Assert.Equal(SeedCount + 2, count);
    }

    // AnyAsync

    [Fact]
    public async Task AnyAsync_ShouldReturnTrue_WhenMatchExists()
    {
        await _repository.AddAsync(CreateUser("alice", "alice@example.com"));

        var result = await _repository.AnyAsync(u => u.Username == "alice");

        Assert.True(result);
    }

    [Fact]
    public async Task AnyAsync_ShouldReturnFalse_WhenNoMatch()
    {
        await _repository.AddAsync(CreateUser());

        var result = await _repository.AnyAsync(u => u.Username == "nonexistent");

        Assert.False(result);
    }

    // GetByUsernameAsync

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnUser_WhenExists()
    {
        await _repository.AddAsync(CreateUser("alice", "alice@example.com"));

        var result = await _repository.GetByUsernameAsync("alice");

        Assert.NotNull(result);
        Assert.Equal("alice", result.Username);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repository.GetByUsernameAsync("nonexistent");

        Assert.Null(result);
    }

    // GetByEmailAsync

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnUser_WhenExists()
    {
        await _repository.AddAsync(CreateUser("alice", "alice@example.com"));

        var result = await _repository.GetByEmailAsync("alice@example.com");

        Assert.NotNull(result);
        Assert.Equal("alice@example.com", result.Email);
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _repository.GetByEmailAsync("nobody@example.com");

        Assert.Null(result);
    }
}
