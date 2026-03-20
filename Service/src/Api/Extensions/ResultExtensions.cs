using Domain.Common;
using Microsoft.AspNetCore.Http;

namespace Api.Extensions;

/// <summary>
/// Provides extension methods for converting <see cref="Result"/> to <see cref="IResult"/>.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a <see cref="Result"/> to a <see cref="IResult"/> with problem details.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <returns>An <see cref="IResult"/> representing the problem details.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is successful.</exception>
    public static IResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Can't convert success result to problem");
        }

        // Use the IsNotFound property to consistently determine status code
        var statusCode = result.IsNotFound
            ? StatusCodes.Status404NotFound
            : (result.Error switch
            {
                { Code: "Error.NullValue" } => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            });

        return Results.Problem(
            statusCode: statusCode,
            title: GetTitleForStatusCode(statusCode),
            type: GetRfcLinkForStatusCode(statusCode),
            extensions: new Dictionary<string, object?>
            {
                { "errors", new[] { result.Error } }
            });
    }

    private static string GetTitleForStatusCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status404NotFound => "Resource Not Found",
        StatusCodes.Status500InternalServerError => "An unexpected error occurred",
        _ => "An error occurred"
    };

    private static string GetRfcLinkForStatusCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        StatusCodes.Status500InternalServerError => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        _ => "https://tools.ietf.org/html/rfc7231"
    };
}