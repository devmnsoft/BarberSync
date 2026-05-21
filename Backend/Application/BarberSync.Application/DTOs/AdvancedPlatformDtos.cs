namespace BarberSync.Application.DTOs;

public record VideoRecognitionRequestDto(Guid AppointmentId, Guid ProfessionalId, string VideoUrl, int DurationSeconds, string DeviceType);

public class AdvancedDashboardWidgetDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public int Width { get; set; } = 4;
    public int Height { get; set; } = 3;
}

public class IntelligentNotificationTriggerDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventKey { get; set; } = string.Empty;
    public string Channel { get; set; } = "push";
    public string TargetType { get; set; } = "client";
    public string Template { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
}

public class BiExportDto
{
    public string Provider { get; set; } = "power-bi";
    public string DatasetName { get; set; } = "barbersync_kpis";
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
    public Dictionary<string, decimal> Kpis { get; set; } = new();
}
