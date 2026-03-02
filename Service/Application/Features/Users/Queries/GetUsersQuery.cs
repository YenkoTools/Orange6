namespace Application.Features.Users.Queries;

/// <summary>
/// Query to retrieve a paginated list of users.
/// </summary>
public record GetUsersQuery(int PageNumber = 1, int PageSize = 10);
