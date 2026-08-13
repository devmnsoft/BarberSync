using BarberSync.Api.Services.Configuration;
using Microsoft.Extensions.Configuration;

namespace BarberSync.Tests;

public sealed class ConfigurationTests
{
    [Fact]
    public void Missing_connection_has_clear_message()
    {
        var configuration = Build([]);

        Assert.Null(DatabaseConnectionResolver.Resolve(configuration));
        Assert.Equal("ConnectionStrings:DefaultConnection não configurada.", DatabaseConnectionResolver.MissingConfigurationMessage);
    }

    [Theory]
    [InlineData("ConnectionStrings:DefaultConnection")]
    [InlineData("ConnectionStrings__DefaultConnection")]
    [InlineData("BARBERSYNC_ConnectionStrings__DefaultConnection")]
    public void Supported_connection_key_is_resolved(string key)
    {
        const string expected = "Host=localhost;Database=barber;Username=test;Password=secret";
        var configuration = Build(new Dictionary<string, string?> { [key] = expected });

        Assert.Equal(expected, DatabaseConnectionResolver.Resolve(configuration));
    }

    private static IConfiguration Build(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();
}
