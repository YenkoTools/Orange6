using Domain.Common;

namespace Domain.Errors;

public static class UserErrors
{
    public static Error UsersNotFound => new(
        "User.UsersNotFound", "No users found");

    public static Error UserByIdNotFound(int id) => new(
        "User.UserByIdNotFound", $"User with ID = '{id}' was not found");

    public static Error UserByUserIdNotFound(string userId) => new(
        "User.UserByUserIdNotFound", $"User with UserId = '{userId}' was not found");

    public static Error UserWithUsernameExists(string username) => new(
        "User.UserWithUsernameExists", $"A user with Username = '{username}' already exists");
}
