namespace Application.Features.Users.Commands;

/// <summary>
/// Represents a command to delete an existing user by their unique identifier.
/// </summary>
public record DeleteUserCommand(int Id);
