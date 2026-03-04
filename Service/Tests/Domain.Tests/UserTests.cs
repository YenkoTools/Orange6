using Domain.Entities;

namespace Domain.Tests;

public class UserTests
{
    [Fact]
    public void User_Username_DefaultsToEmptyString()
    {
        var user = new User();
        Assert.Equal(string.Empty, user.Username);
    }

    [Fact]
    public void User_Email_DefaultsToEmptyString()
    {
        var user = new User();
        Assert.Equal(string.Empty, user.Email);
    }

    [Fact]
    public void User_FirstName_DefaultsToEmptyString()
    {
        var user = new User();
        Assert.Equal(string.Empty, user.FirstName);
    }

    [Fact]
    public void User_LastName_DefaultsToEmptyString()
    {
        var user = new User();
        Assert.Equal(string.Empty, user.LastName);
    }

    [Fact]
    public void User_Username_CanBeSet()
    {
        var user = new User { Username = "jdoe" };
        Assert.Equal("jdoe", user.Username);
    }

    [Fact]
    public void User_Email_CanBeSet()
    {
        var user = new User { Email = "jdoe@example.com" };
        Assert.Equal("jdoe@example.com", user.Email);
    }

    [Fact]
    public void User_FirstName_CanBeSet()
    {
        var user = new User { FirstName = "John" };
        Assert.Equal("John", user.FirstName);
    }

    [Fact]
    public void User_LastName_CanBeSet()
    {
        var user = new User { LastName = "Doe" };
        Assert.Equal("Doe", user.LastName);
    }

    [Fact]
    public void User_IsAssignableFrom_Entity()
    {
        var user = new User();
        Assert.IsAssignableFrom<Entity>(user);
    }
}
