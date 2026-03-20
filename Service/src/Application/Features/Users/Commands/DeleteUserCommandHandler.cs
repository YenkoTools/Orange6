using Application.Abstractions;
using Application.Interfaces;
using Domain.Common;

namespace Application.Features.Users.Commands;

/// <summary>
/// Handles the deletion of an existing user by their unique identifier.
/// </summary>
public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand, Result>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.Id, cancellationToken);

        if (user is null)
        {
            return Result.NotFound(new Error("NotFound.User", $"User with id {command.Id} was not found."));
        }

        await _userRepository.DeleteAsync(user, cancellationToken);
        return Result.Success();
    }
}
