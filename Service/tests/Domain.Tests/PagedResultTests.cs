using Domain.Common;

namespace Domain.Tests;

public class PagedResultTests
{
    [Fact]
    public void PagedResult_Constructor_SetsItems()
    {
        var items = new List<int> { 1, 2, 3 };
        var result = new PagedResult<int>(items, 10, 3, 1);
        Assert.Equal(items, result.Items);
    }

    [Fact]
    public void PagedResult_Constructor_SetsTotalCount()
    {
        var result = new PagedResult<int>(new List<int>(), 25, 10, 1);
        Assert.Equal(25, result.TotalCount);
    }

    [Fact]
    public void PagedResult_Constructor_SetsPageSize()
    {
        var result = new PagedResult<int>(new List<int>(), 25, 10, 1);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public void PagedResult_Constructor_SetsPageNumber()
    {
        var result = new PagedResult<int>(new List<int>(), 25, 10, 2);
        Assert.Equal(2, result.PageNumber);
    }

    [Fact]
    public void PagedResult_TotalPages_CalculatedCorrectly()
    {
        var result = new PagedResult<int>(new List<int>(), 25, 10, 1);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public void PagedResult_TotalPages_ExactDivision_CalculatedCorrectly()
    {
        var result = new PagedResult<int>(new List<int>(), 20, 10, 1);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public void PagedResult_TotalPages_ZeroPageSize_ReturnsZero()
    {
        var result = new PagedResult<int>(new List<int>(), 20, 0, 1);
        Assert.Equal(0, result.TotalPages);
    }

    [Fact]
    public void PagedResult_Constructor_NullItems_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PagedResult<int>(null!, 10, 5, 1));
    }

    [Fact]
    public void PagedResult_Create_ReturnsPagedResult()
    {
        var items = new List<string> { "a", "b" };
        var result = PagedResult<string>.Create(items, 10, 5, 1);
        Assert.NotNull(result);
        Assert.Equal(items, result.Items);
        Assert.Equal(10, result.TotalCount);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(1, result.PageNumber);
    }

    [Fact]
    public void PagedResult_TotalPages_SingleItem_ReturnsOne()
    {
        var result = new PagedResult<int>(new List<int> { 1 }, 1, 10, 1);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public void PagedResult_TotalPages_EmptyCollection_ReturnsZeroPages()
    {
        var result = new PagedResult<int>(new List<int>(), 0, 10, 1);
        Assert.Equal(0, result.TotalPages);
    }
}
