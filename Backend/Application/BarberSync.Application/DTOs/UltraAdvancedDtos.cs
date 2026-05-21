namespace BarberSync.Application.DTOs;

public class VideoRecognitionInsightDto
{
    public Guid AppointmentId { get; set; }
    public Guid ProfessionalId { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
    public int DurationSeconds { get; set; }
    public decimal PostureScore { get; set; }
    public decimal InstrumentHandlingScore { get; set; }
    public decimal TechniqueAdherenceScore { get; set; }
    public IEnumerable<ServiceRecognitionEventDto> RecognizedServices { get; set; } = Array.Empty<ServiceRecognitionEventDto>();
    public IEnumerable<string> UpsellSuggestions { get; set; } = Array.Empty<string>();
}

public record ServiceRecognitionEventDto(string ServiceName, decimal Confidence, string Instrument, string Technique);
public record AutomationTriggerStateDto(string TriggerKey, bool Active, string Action);

public class PredictiveAnalyticsSummaryDto
{
    public DateTime GeneratedAtUtc { get; set; }
    public IEnumerable<string> PeakHours { get; set; } = Array.Empty<string>();
    public IEnumerable<string> TopServices { get; set; } = Array.Empty<string>();
    public IEnumerable<string> StockRiskItems { get; set; } = Array.Empty<string>();
    public decimal OccupancyProjection { get; set; }
    public decimal ProfitProjection { get; set; }
    public decimal SatisfactionProjection { get; set; }
}
