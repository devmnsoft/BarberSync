namespace BarberSync.Application.DTOs;

public class NotificationAlertDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Channel { get; set; } = "push";
    public string AudienceType { get; set; } = "client";
    public Guid AudienceId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string Status { get; set; } = "queued";
    public DateTime TriggerAtUtc { get; set; } = DateTime.UtcNow;
}
