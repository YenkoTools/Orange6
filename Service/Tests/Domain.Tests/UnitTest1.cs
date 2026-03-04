using Domain.Entities;

namespace Domain.Tests;

public class EntityTests
{
    [Fact]
    public void Entity_Id_DefaultsToZero()
    {
        var user = new User();
        Assert.Equal(0, user.Id);
    }

    [Fact]
    public void Entity_CreatedAt_DefaultsToMinValue()
    {
        var user = new User();
        Assert.Equal(default(DateTime), user.CreatedAt);
    }

    [Fact]
    public void Entity_UpdatedAt_DefaultsToMinValue()
    {
        var user = new User();
        Assert.Equal(default(DateTime), user.UpdatedAt);
    }

    [Fact]
    public void Entity_Id_CanBeSet()
    {
        var user = new User { Id = 42 };
        Assert.Equal(42, user.Id);
    }

    [Fact]
    public void Entity_CreatedAt_CanBeSet()
    {
        var now = DateTime.UtcNow;
        var user = new User { CreatedAt = now };
        Assert.Equal(now, user.CreatedAt);
    }

    [Fact]
    public void Entity_UpdatedAt_CanBeSet()
    {
        var now = DateTime.UtcNow;
        var user = new User { UpdatedAt = now };
        Assert.Equal(now, user.UpdatedAt);
    }
}