using Application.Abstractions;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;

namespace Application.Features.Users.Commands;

/// <summary>
/// Handles the update of an existing user.
/// </summary>
public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, Result<User>>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<User>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.Id, cancellationToken);

        if (user is null)
        {
            return Result<User>.NotFound(new Error("NotFound.User", $"User with id {command.Id} was not found."));
        }

        user.Username = command.Username;
        user.Email = command.Email;
        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        return Result<User>.Success(user);
    }
}
