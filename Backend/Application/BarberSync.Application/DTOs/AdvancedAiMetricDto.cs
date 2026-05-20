namespace BarberSync.Application.DTOs;

public class AdvancedAiMetricDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AppointmentId { get; set; }
    public Guid ProfessionalId { get; set; }
    public string InputType { get; set; } = "video";
    public string PredictedService { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public int ProcessingTimeMs { get; set; }
    public int ServiceDurationMinutes { get; set; }
    public DateTime CapturedAtUtc { get; set; } = DateTime.UtcNow;
    public string EfficiencyAlert { get; set; } = string.Empty;
    public List<string> UpsellSuggestions { get; set; } = new();
}
