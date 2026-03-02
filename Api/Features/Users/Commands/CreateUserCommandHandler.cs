using Api.Common;
using Api.Domain.Common;
using Api.Domain.Entities;
using Api.Interfaces;

namespace Api.Features.Users.Commands;

/// <summary>
/// Handles the creation of a new user.
/// </summary>
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Result<User>>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateUserCommandHandler"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Handles the create user command.
    /// </summary>
    /// <param name="command">The create user command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the created user if successful.</returns>
    public async Task<Result<User>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Username = command.Username,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);
        return Result<User>.Success(user);
    }
}