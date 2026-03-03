using Domain.Errors;

namespace Domain.Tests;

public class UserErrorsTests
{
    [Fact]
    public void UserErrors_UsersNotFound_HasExpectedCode()
    {
        var error = UserErrors.UsersNotFound;
        Assert.Equal("User.UsersNotFound", error.Code);
    }

    [Fact]
    public void UserErrors_UsersNotFound_HasExpectedDescription()
    {
        var error = UserErrors.UsersNotFound;
        Assert.Equal("No users found", error.Description);
    }

    [Fact]
    public void UserErrors_UserByIdNotFound_HasExpectedCode()
    {
        var error = UserErrors.UserByIdNotFound(5);
        Assert.Equal("User.UserByIdNotFound", error.Code);
    }

    [Fact]
    public void UserErrors_UserByIdNotFound_HasExpectedDescription()
    {
        var error = UserErrors.UserByIdNotFound(5);
        Assert.Equal("User with ID = '5' was not found", error.Description);
    }

    [Fact]
    public void UserErrors_UserByUserIdNotFound_HasExpectedCode()
    {
        var error = UserErrors.UserByUserIdNotFound("abc123");
        Assert.Equal("User.UserByUserIdNotFound", error.Code);
    }

    [Fact]
    public void UserErrors_UserByUserIdNotFound_HasExpectedDescription()
    {
        var error = UserErrors.UserByUserIdNotFound("abc123");
        Assert.Equal("User with UserId = 'abc123' was not found", error.Description);
    }
}
