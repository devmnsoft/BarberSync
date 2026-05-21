namespace BarberSync.Application.DTOs;

public sealed record PlatformCommandCenterDto(
    DateTime GeneratedAtUtc,
    KpiBoardDto Kpis,
    IReadOnlyCollection<AutomationQueueDto> AutomationQueues,
    IReadOnlyCollection<SecurityAlertDto> SecurityAlerts,
    IReadOnlyCollection<AiRuntimeAlertDto> AiRuntimeAlerts,
    IReadOnlyCollection<EngagementActionDto> EngagementActions);

public sealed record KpiBoardDto(
    decimal DailyRevenue,
    decimal OccupancyRate,
    decimal Nps,
    decimal UpsellConversionRate,
    decimal StockRiskRate);

public sealed record AutomationQueueDto(string Queue, int Pending, int Running, int FailedLastHour);

public sealed record SecurityAlertDto(string Severity, string Module, string Message, DateTime CreatedAtUtc);

public sealed record AiRuntimeAlertDto(string AlertType, string ProfessionalName, decimal Confidence, string Action, DateTime CreatedAtUtc);

public sealed record EngagementActionDto(string Audience, string Channel, string Campaign, string SuggestedTrigger);
