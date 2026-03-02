using Orange6.Application.Abstractions;
using Orange6.Application.Interfaces;
using Orange6.Domain.Common;
using Orange6.Domain.Entities;
using Orange6.Domain.Errors;

namespace Orange6.Application.Features.Users.Queries;

/// <summary>
/// Handles the GetUsersQuery to retrieve paginated users.
/// </summary>
public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, Result<PagedResult<User>>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PagedResult<User>>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var pagedResult = await _userRepository.GetPagedAsync(
                query.PageNumber,
                query.PageSize,
                cancellationToken);

            if (pagedResult.Items == null || !pagedResult.Items.Any())
                return Result<PagedResult<User>>.Failure(UserErrors.UsersNotFound);

            return Result<PagedResult<User>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<User>>.Failure(
                new Error(ex.Message, "An error occurred while retrieving users"));
        }
    }
}
