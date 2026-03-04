using Domain.Common;

namespace Domain.Tests;

public class ErrorTests
{
    [Fact]
    public void Error_None_HasEmptyCodeAndDescription()
    {
        Assert.Equal(string.Empty, Error.None.Code);
        Assert.Equal(string.Empty, Error.None.Description);
    }

    [Fact]
    public void Error_NullValue_HasExpectedCode()
    {
        Assert.Equal("Error.NullValue", Error.NullValue.Code);
    }

    [Fact]
    public void Error_NullValue_HasExpectedDescription()
    {
        Assert.Equal("Null value was provided", Error.NullValue.Description);
    }

    [Fact]
    public void Error_NotFound_HasExpectedCode()
    {
        Assert.Equal("Error.NotFound", Error.NotFound.Code);
    }

    [Fact]
    public void Error_NotFound_HasExpectedDescription()
    {
        Assert.Equal("The requested resource was not found", Error.NotFound.Description);
    }

    [Fact]
    public void Error_Constructor_SetsCodeAndDescription()
    {
        var error = new Error("Test.Code", "Test description");
        Assert.Equal("Test.Code", error.Code);
        Assert.Equal("Test description", error.Description);
    }

    [Fact]
    public void Error_ToResult_ReturnsFailureResult()
    {
        var error = new Error("Test.Code", "Test description");
        var result = error.ToResult();
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Error_ImplicitConversion_ReturnsFailureResult()
    {
        var error = new Error("Test.Code", "Test description");
        Result result = error;
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Error_Equality_SameValues_AreEqual()
    {
        var error1 = new Error("Same.Code", "Same description");
        var error2 = new Error("Same.Code", "Same description");
        Assert.Equal(error1, error2);
    }

    [Fact]
    public void Error_Equality_DifferentValues_AreNotEqual()
    {
        var error1 = new Error("Code.One", "Description one");
        var error2 = new Error("Code.Two", "Description two");
        Assert.NotEqual(error1, error2);
    }
}
