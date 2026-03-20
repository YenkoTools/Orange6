using Application.Interfaces;
using Dapper;
using Domain.Common;
using Domain.Entities;
using Infrastructure.Data;
using System.Linq.Expressions;

namespace Infrastructure.Repositories;

/// <summary>
/// SQLite implementation of IUserRepository using Dapper ORM.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IDatabaseFactory _databaseFactory;

    public UserRepository(IDatabaseFactory databaseFactory)
    {
        _databaseFactory = databaseFactory;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _databaseFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _databaseFactory.CreateConnection();
        return await connection.QueryAsync<User>("SELECT * FROM Users");
    }

    public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        return all.Where(predicate.Compile());
    }

    public async Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        using var connection = _databaseFactory.CreateConnection();
        var offset = (page - 1) * pageSize;
        var items = await connection.QueryAsync<User>(
            "SELECT * FROM Users LIMIT @PageSize OFFSET @Offset",
            new { PageSize = pageSize, Offset = offset });
        var totalCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users");
        return new PagedResult<User>(items, totalCount, pageSize, page);
    }

    public async Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        using var connection = _databaseFactory.CreateConnection();
        var id = await connection.ExecuteScalarAsync<int>("""
            INSERT INTO Users (Username, Email, FirstName, LastName, CreatedAt, UpdatedAt)
            VALUES (@Username, @Email, @FirstName, @LastName, @CreatedAt, @UpdatedAt);
            SELECT last_insert_rowid();
            """,
            entity);
        entity.Id = id;
        return entity;
    }

    public async Task UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        using var connection = _databaseFactory.CreateConnection();
        await connection.ExecuteAsync("""
            UPDATE Users
            SET Username = @Username, Email = @Email, FirstName = @FirstName,
                LastName = @LastName, UpdatedAt = @UpdatedAt
            WHERE Id = @Id
            """,
            entity);
    }

    public async Task DeleteAsync(User entity, CancellationToken cancellationToken = default)
    {
        using var connection = _databaseFactory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM Users WHERE Id = @Id", new { entity.Id });
    }

    public async Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _databaseFactory.CreateConnection();
        await connection.ExecuteAsync("DELETE FROM Users WHERE Id = @Id", new { Id = id });
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _databaseFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Users");
    }

    public async Task<bool> AnyAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        return all.Any(predicate.Compile());
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        using var connection = _databaseFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Username = @Username", new { Username = username });
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var connection = _databaseFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Email = @Email", new { Email = email });
    }
}
