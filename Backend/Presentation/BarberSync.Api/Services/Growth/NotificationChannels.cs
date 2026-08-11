namespace BarberSync.Api.Services.Growth;

public sealed record NotificationMessage(string Recipient, string Subject, string Body);
public sealed record NotificationDelivery(bool Delivered, string Message, string? ProviderId = null);

public interface INotificationChannel { string Name { get; } Task<NotificationDelivery> SendAsync(NotificationMessage message, CancellationToken cancellationToken); }
public interface IWhatsAppChannel : INotificationChannel { }
public interface IEmailChannel : INotificationChannel { }
public interface ISmsChannel : INotificationChannel { }
public interface IWhatsAppProvider : IWhatsAppChannel { }
public interface IEmailProvider : IEmailChannel { }
public interface ISmsProvider : ISmsChannel { }
public interface INotificationDispatcher { Task<NotificationDelivery> DispatchAsync(string channel, NotificationMessage message, CancellationToken cancellationToken); }

public sealed class UnconfiguredWhatsAppProvider : IWhatsAppProvider
{
    public string Name => "WhatsApp";
    public Task<NotificationDelivery> SendAsync(NotificationMessage message, CancellationToken cancellationToken) =>
        Task.FromResult(new NotificationDelivery(false, "Canal não configurado."));
}

public sealed class UnconfiguredEmailProvider : IEmailProvider { public string Name => "Email"; public Task<NotificationDelivery> SendAsync(NotificationMessage m, CancellationToken ct) => Task.FromResult(new NotificationDelivery(false, "Canal não configurado.")); }
public sealed class UnconfiguredSmsProvider : ISmsProvider { public string Name => "SMS"; public Task<NotificationDelivery> SendAsync(NotificationMessage m, CancellationToken ct) => Task.FromResult(new NotificationDelivery(false, "Canal não configurado.")); }
public sealed class NotificationDispatcher(IWhatsAppProvider whatsapp, IEmailProvider email, ISmsProvider sms) : INotificationDispatcher
{
    public Task<NotificationDelivery> DispatchAsync(string channel, NotificationMessage message, CancellationToken ct) => channel.ToLowerInvariant() switch
    { "whatsapp" => whatsapp.SendAsync(message,ct), "email" => email.SendAsync(message,ct), "sms" => sms.SendAsync(message,ct), _ => Task.FromResult(new NotificationDelivery(false,"Canal não configurado.")) };
}
