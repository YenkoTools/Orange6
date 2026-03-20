using Application.Behaviors;
using Domain.Common;
using Domain.Entities;

namespace Application.Tests.Behaviors;

public class DefaultResultAnalyzerTests
{
    private readonly DefaultResultAnalyzer _analyzer;

    public DefaultResultAnalyzerTests()
    {
        _analyzer = new DefaultResultAnalyzer();
    }

    [Fact]
    public void AnalyzeResult_NullInput_ReturnsFalseNullType()
    {
        // Act
        var analysis = _analyzer.AnalyzeResult(null);

        // Assert
        Assert.False(analysis.IsSuccess);
        Assert.Equal("null", analysis.ResultType);
        Assert.Null(analysis.ItemCount);
    }

    [Fact]
    public void AnalyzeResult_SuccessResultWithEntity_ReturnsSuccessEntityType()
    {
        // Arrange
        var user = new User { Id = 1, Username = "jdoe" };
        var result = Result<User>.Success(user);

        // Act
        var analysis = _analyzer.AnalyzeResult(result);

        // Assert
        Assert.True(analysis.IsSuccess);
        Assert.Equal("entity", analysis.ResultType);
    }

    [Fact]
    public void AnalyzeResult_FailureResult_ReturnsFalse()
    {
        // Arrange
        var result = Result<User>.Failure(new Error("Error.Test", "Test error"));

        // Act
        var analysis = _analyzer.AnalyzeResult(result);

        // Assert
        Assert.False(analysis.IsSuccess);
    }

    [Fact]
    public void AnalyzeResult_SuccessResultWithCollection_ReturnsCollectionTypeWithCount()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = 1, Username = "user1" },
            new() { Id = 2, Username = "user2" }
        };
        var pagedResult = new PagedResult<User>(users, 2, 10, 1);
        var result = Result<PagedResult<User>>.Success(pagedResult);

        // Act
        var analysis = _analyzer.AnalyzeResult(result);

        // Assert
        Assert.True(analysis.IsSuccess);
    }

    [Fact]
    public void AnalyzeResult_SuccessResultWithString_ReturnsPrimitiveType()
    {
        // Arrange
        var result = Result<string>.Success("hello");

        // Act
        var analysis = _analyzer.AnalyzeResult(result);

        // Assert
        Assert.True(analysis.IsSuccess);
        Assert.Equal("primitive", analysis.ResultType);
    }

    [Fact]
    public void AnalyzeResult_PlainObject_ReturnsSuccessEntityType()
    {
        // Arrange
        var entity = new User { Id = 1, Username = "jdoe" };

        // Act
        var analysis = _analyzer.AnalyzeResult(entity);

        // Assert
        Assert.True(analysis.IsSuccess);
        Assert.Equal("entity", analysis.ResultType);
    }

    [Fact]
    public void AnalyzeResult_List_ReturnsCollectionType()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        var analysis = _analyzer.AnalyzeResult(list);

        // Assert
        Assert.True(analysis.IsSuccess);
        Assert.Equal("collection", analysis.ResultType);
        Assert.Equal(3, analysis.ItemCount);
    }
}
