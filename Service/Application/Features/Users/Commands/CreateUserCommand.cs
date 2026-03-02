namespace Application.Features.Users.Commands;

/// <summary>
/// Represents a command to create a new user in the system.
/// </summary>
public record CreateUserCommand(
    string Username,
    string Email,
    string FirstName,
    string LastName
);
