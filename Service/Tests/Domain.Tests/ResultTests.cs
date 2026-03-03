using Domain.Common;

namespace Domain.Tests;

public class ResultTests
{
    [Fact]
    public void Result_Success_IsSuccessIsTrue()
    {
        var result = Result.Success();
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Result_Success_ErrorMessageIsEmpty()
    {
        var result = Result.Success();
        Assert.Equal(string.Empty, result.ErrorMessage);
    }

    [Fact]
    public void Result_Failure_WithMessage_IsSuccessIsFalse()
    {
        var result = Result.Failure("Something went wrong");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Result_Failure_WithMessage_SetsErrorMessage()
    {
        const string message = "Something went wrong";
        var result = Result.Failure(message);
        Assert.Equal(message, result.ErrorMessage);
    }

    [Fact]
    public void Result_Failure_WithError_IsSuccessIsFalse()
    {
        var error = new Error("Test.Error", "Test error");
        var result = Result.Failure(error);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Result_Failure_WithError_SetsError()
    {
        var error = new Error("Test.Error", "Test error");
        var result = Result.Failure(error);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Result_NotFound_WithError_IsSuccessIsFalse()
    {
        var error = new Error("NotFound.Resource", "Resource not found");
        var result = Result.NotFound(error);
        Assert.False(result.IsSuccess);
        Assert.True(result.IsNotFound);
    }

    [Fact]
    public void Result_NotFound_WithResourceName_IsNotFound()
    {
        var result = Result.NotFound("user");
        Assert.False(result.IsSuccess);
        Assert.True(result.IsNotFound);
    }

    [Fact]
    public void Result_NotFound_DefaultResourceName_IsNotFound()
    {
        var result = Result.NotFound();
        Assert.False(result.IsSuccess);
        Assert.True(result.IsNotFound);
    }

    [Fact]
    public void Result_Success_IsNotFound_ReturnsFalse()
    {
        var result = Result.Success();
        Assert.False(result.IsNotFound);
    }

    [Fact]
    public void Result_Failure_WithNonNotFoundError_IsNotFoundIsFalse()
    {
        var error = new Error("General.Error", "Some error");
        var result = Result.Failure(error);
        Assert.False(result.IsNotFound);
    }

    [Fact]
    public void Result_ToString_Success_ReturnsSuccessString()
    {
        var result = Result.Success();
        Assert.Equal("Success", result.ToString());
    }

    [Fact]
    public void Result_ToString_Failure_ReturnsFailureString()
    {
        var result = Result.Failure("Some error");
        Assert.StartsWith("Failure:", result.ToString());
    }

    [Fact]
    public void Result_Failure_WithErrorNone_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure(Error.None));
    }
}
