namespace Api.Features.Users.Commands;

/// <summary>
/// Represents a command to create a new user in the system.
/// </summary>
/// <param name="Username">The username for the new user.</param>
/// <param name="Email">The email address for the new user.</param>
/// <param name="FirstName">The first name of the new user.</param>
/// <param name="LastName">The last name of the new user.</param>
public record CreateUserCommand(
    string Username,
    string Email,
    string FirstName,
    string LastName
);
