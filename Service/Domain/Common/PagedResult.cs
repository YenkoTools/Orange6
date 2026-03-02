namespace Domain.Common;

/// <summary>
/// Represents a paginated result containing items and pagination metadata.
/// </summary>
/// <typeparam name="T">The type of items in the result.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// The collection of items for the current page.
    /// </summary>
    public IEnumerable<T> Items { get; }

    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    public int TotalPages { get; }

    public PagedResult(IEnumerable<T> items, int totalCount, int pageSize, int pageNumber)
    {
        Items = items ?? throw new ArgumentNullException(nameof(items));
        TotalCount = totalCount;
        PageSize = pageSize;
        PageNumber = pageNumber;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
    }

    /// <summary>
    /// Creates a successful paged result.
    /// </summary>
    public static PagedResult<T> Create(IEnumerable<T> items, int totalCount, int pageSize, int pageNumber)
    {
        return new PagedResult<T>(items, totalCount, pageSize, pageNumber);
    }
}
