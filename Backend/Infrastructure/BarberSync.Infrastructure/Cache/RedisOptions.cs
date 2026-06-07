namespace BarberSync.Infrastructure.Cache;

public sealed class RedisOptions
{
    public string ConnectionString { get; set; } = "";

    public bool Enabled { get; set; }
}
