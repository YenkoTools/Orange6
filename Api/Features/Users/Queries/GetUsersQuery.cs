namespace Api.Features.Users.Queries;

/// <summary>
/// Query to retrieve a paginated list of users.
/// </summary>
/// <param name="PageNumber">The page number (1-based).</param>
/// <param name="PageSize">The number of items per page.</param>
public record GetUsersQuery(int PageNumber = 1, int PageSize = 10);