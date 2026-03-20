namespace Domain.Entities;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User : Entity
{
    /// <summary>
    /// Gets or sets the username for the user.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address for the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    public string LastName { get; set; } = string.Empty;
}
