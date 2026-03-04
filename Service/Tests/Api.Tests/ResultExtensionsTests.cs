using Api.Extensions;
using Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Tests;

public class ResultExtensionsTests
{
    [Fact]
    public void ToProblemDetails_ThrowsInvalidOperationException_WhenResultIsSuccess()
    {
        var result = Result.Success();

        Assert.Throws<InvalidOperationException>(() => result.ToProblemDetails());
    }

    [Fact]
    public void ToProblemDetails_Returns500_WhenResultIsGenericFailure()
    {
        var error = new Error("Error.SomethingWentWrong", "Something went wrong");
        var result = Result.Failure(error);

        var problemResult = result.ToProblemDetails();

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(problemResult);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public void ToProblemDetails_Returns404_WhenResultIsNotFound()
    {
        var result = Result.NotFound("User");

        var problemResult = result.ToProblemDetails();

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(problemResult);
        Assert.Equal(StatusCodes.Status404NotFound, statusResult.StatusCode);
    }

    [Fact]
    public void ToProblemDetails_Returns400_WhenResultHasNullValueError()
    {
        var result = Result.Failure(Error.NullValue);

        var problemResult = result.ToProblemDetails();

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(problemResult);
        Assert.Equal(StatusCodes.Status400BadRequest, statusResult.StatusCode);
    }

    [Fact]
    public void ToProblemDetails_Returns404_WhenResultHasNotFoundError()
    {
        var result = Result.Failure(Error.NotFound);

        var problemResult = result.ToProblemDetails();

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(problemResult);
        Assert.Equal(StatusCodes.Status404NotFound, statusResult.StatusCode);
    }

    [Fact]
    public void ToProblemDetails_Returns404_WhenErrorCodeStartsWithNotFound()
    {
        var error = new Error("NotFound.User", "User not found");
        var result = Result.Failure(error);

        var problemResult = result.ToProblemDetails();

        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(problemResult);
        Assert.Equal(StatusCodes.Status404NotFound, statusResult.StatusCode);
    }

    [Fact]
    public void ToProblemDetails_IncludesErrorInExtensions_WhenResultIsFailure()
    {
        var error = new Error("Error.SomethingWentWrong", "Something went wrong");
        var result = Result.Failure(error);

        var problemResult = result.ToProblemDetails();

        var httpResult = Assert.IsType<ProblemHttpResult>(problemResult);
        Assert.NotNull(httpResult.ProblemDetails.Extensions);
        Assert.True(httpResult.ProblemDetails.Extensions.ContainsKey("errors"));
    }
}
