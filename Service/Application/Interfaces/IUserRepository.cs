using Domain.Common;
using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// Defines the contract for the user repository.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
