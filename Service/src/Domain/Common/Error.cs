namespace Domain.Common;

/// <summary>
/// Represents an error with a code and description.
/// </summary>
/// <param name="Code">The unique code identifying the error.</param>
/// <param name="Description">A detailed description of the error.</param>
public record Error(string Code, string Description)
{
    /// <summary>
    /// Represents the absence of an error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Represents an error indicating that a null value was provided.
    /// </summary>
    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided");

    /// <summary>
    /// Represents an error indicating that a requested resource was not found.
    /// </summary>
    public static readonly Error NotFound = new("Error.NotFound", "The requested resource was not found");

    /// <summary>
    /// Implicitly converts an <see cref="Error"/> to a <see cref="Result"/> object.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>A failure <see cref="Result"/> containing the error.</returns>
    public static implicit operator Result(Error error) => Result.Failure(error);

    /// <summary>
    /// Converts the current <see cref="Error"/> to a failure <see cref="Result"/>.
    /// </summary>
    /// <returns>A failure <see cref="Result"/> containing the error.</returns>
    public Result ToResult() => Result.Failure(this);
}
