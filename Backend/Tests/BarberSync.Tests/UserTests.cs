using BarberSync.Domain.Entities;

namespace BarberSync.Tests;

public class UserTests
{
    [Fact]
    public void Should_Create_User()
    {
        var user = new User("Admin", "admin-instalacao@exemplo.com", "hash", "Admin");
        Assert.Equal("Admin", user.Role);
    }
}
