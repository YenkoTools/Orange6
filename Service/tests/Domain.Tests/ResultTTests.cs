using Domain.Common;

namespace Domain.Tests;

public class ResultTTests
{
    [Fact]
    public void ResultT_Success_IsSuccessIsTrue()
    {
        var result = Result<int>.Success(42);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ResultT_Success_ValueIsSet()
    {
        var result = Result<int>.Success(42);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ResultT_Failure_IsSuccessIsFalse()
    {
        var error = new Error("Test.Error", "Test error");
        var result = Result<int>.Failure(error);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ResultT_Failure_AccessValue_ThrowsInvalidOperationException()
    {
        var error = new Error("Test.Error", "Test error");
        var result = Result<int>.Failure(error);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void ResultT_ImplicitConversion_NonNull_IsSuccess()
    {
        Result<string> result = "hello";
        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void ResultT_ImplicitConversion_Null_IsFailure()
    {
        Result<string> result = (string?)null;
        Assert.False(result.IsSuccess);
        Assert.Equal(Error.NullValue, result.Error);
    }

    [Fact]
    public void ResultT_NotFound_WithError_IsNotFoundIsTrue()
    {
        var error = new Error("NotFound.Item", "Item not found");
        var result = Result<string>.NotFound(error);
        Assert.False(result.IsSuccess);
        Assert.True(result.IsNotFound);
    }

    [Fact]
    public void ResultT_NotFound_WithResourceName_IsNotFoundIsTrue()
    {
        var result = Result<string>.NotFound("item");
        Assert.False(result.IsSuccess);
        Assert.True(result.IsNotFound);
    }

    [Fact]
    public void ResultT_Success_ErrorIsNone()
    {
        var result = Result<int>.Success(1);
        Assert.Equal(Error.None, result.Error);
    }
}
