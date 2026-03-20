using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using System.Linq.Expressions;

namespace Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of IUserRepository for development and testing.
/// Replace with an EF Core or other persistence implementation for production.
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = [];
    private int _nextId = 1;

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IEnumerable<User>>(_users);

    public Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default) =>
        Task.FromResult<IEnumerable<User>>(_users.AsQueryable().Where(predicate).ToList());

    public Task<PagedResult<User>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var items = _users.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var result = new PagedResult<User>(items, _users.Count, pageSize, page);
        return Task.FromResult(result);
    }

    public Task<User> AddAsync(User entity, CancellationToken cancellationToken = default)
    {
        entity.Id = _nextId++;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        _users.Add(entity);
        return Task.FromResult(entity);
    }

    public Task UpdateAsync(User entity, CancellationToken cancellationToken = default)
    {
        var index = _users.FindIndex(u => u.Id == entity.Id);
        if (index >= 0)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _users[index] = entity;
        }
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User entity, CancellationToken cancellationToken = default)
    {
        _users.Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user != null) _users.Remove(user);
        return Task.CompletedTask;
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.Count);

    public Task<bool> AnyAsync(Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.AsQueryable().Any(predicate));

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Username == username));

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Email == email));
}
