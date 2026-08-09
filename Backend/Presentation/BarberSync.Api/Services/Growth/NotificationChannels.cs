namespace BarberSync.Api.Services.Growth;

public sealed record NotificationMessage(string Recipient, string Subject, string Body);
public sealed record NotificationDelivery(bool Delivered, string Message, string? ProviderId = null);

public interface INotificationChannel { string Name { get; } Task<NotificationDelivery> SendAsync(NotificationMessage message, CancellationToken cancellationToken); }
public interface IWhatsAppChannel : INotificationChannel { }
public interface IEmailChannel : INotificationChannel { }
public interface ISmsChannel : INotificationChannel { }

public sealed class UnconfiguredWhatsAppChannel(IHostEnvironment environment) : IWhatsAppChannel
{
    public string Name => "WhatsApp";
    public Task<NotificationDelivery> SendAsync(NotificationMessage message, CancellationToken cancellationToken) =>
        Task.FromResult(environment.IsDevelopment()
            ? new NotificationDelivery(true, "Mensagem aceita pelo provider de desenvolvimento.", $"DEV-{Guid.NewGuid():N}")
            : new NotificationDelivery(false, "Canal não configurado"));
}
