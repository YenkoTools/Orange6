using Api.Domain.Common;

namespace Api.Domain.Errors;

public static class UserErrors
{
    public static Error UsersNotFound => new(
        "User.UsersNotFound", "No users found");

    public static Error UserByIdNotFound(int id) => new(
        "User.UserByIdNotFound", $"User with ID = '{id}' was not found");

    public static Error UserByUserIdNotFound(string userId) => new(
        "User.UserByUserIdNotFound", $"User with UserId = '{userId}' was not found");
}

