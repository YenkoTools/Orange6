namespace Application.Features.Users.Commands;

/// <summary>
/// Represents a command to update an existing user in the system.
/// </summary>
public record UpdateUserCommand(
    int Id,
    string Username,
    string Email,
    string FirstName,
    string LastName
);
