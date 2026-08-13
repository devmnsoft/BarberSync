namespace BarberSync.Api.Services.Configuration;

public static class DatabaseConnectionResolver
{
    public const string MissingConfigurationMessage = "ConnectionStrings:DefaultConnection não configurada.";

    public static string? Resolve(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return FirstConfigured(
            configuration.GetConnectionString("DefaultConnection"),
            configuration["BARBERSYNC_ConnectionStrings__DefaultConnection"],
            configuration["ConnectionStrings__DefaultConnection"]);
    }

    private static string? FirstConfigured(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
}
