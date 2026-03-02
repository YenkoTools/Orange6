using Application.Abstractions;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;

namespace Application.Features.Users.Commands;

/// <summary>
/// Handles the creation of a new user.
/// </summary>
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Result<User>>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

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
