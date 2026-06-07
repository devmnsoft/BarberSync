namespace BarberSync.Infrastructure.Messaging;

public sealed class MessagingOptions
{
    public string Provider { get; set; } = "InMemory";

    public string ConnectionString { get; set; } = "";

    public bool Enabled { get; set; }
}
