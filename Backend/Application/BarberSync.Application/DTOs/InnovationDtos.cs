namespace BarberSync.Application.DTOs;

public class ProfessionalPerformanceDto
{
    public Guid ProfessionalId { get; set; }
    public decimal AverageServiceMinutes { get; set; }
    public decimal EfficiencyScore { get; set; }
    public decimal PrecisionScore { get; set; }
    public int CompletedServices { get; set; }
}

public class UpsellRecommendationDto
{
    public Guid ClientId { get; set; }
    public string RecommendedService { get; set; } = string.Empty;
    public decimal ConversionProbability { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class SmartScheduleSuggestionDto
{
    public Guid ClientId { get; set; }
    public Guid ServiceId { get; set; }
    public DateTime SuggestedStartUtc { get; set; }
    public DateTime SuggestedEndUtc { get; set; }
    public string Explanation { get; set; } = string.Empty;
}

public class SmartAlertDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = "info";
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
