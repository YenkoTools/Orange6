using Application.Abstractions;
using Application.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Errors;

namespace Application.Features.Users.Queries;

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, Result<User>>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<User>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(query.UserId, cancellationToken);

            if (user == null)
                return Result<User>.Failure(UserErrors.UserByIdNotFound(query.UserId));

            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<User>.Failure(new Error(ex.Message, "An error occurred while handling the user query"));
        }
    }
}
