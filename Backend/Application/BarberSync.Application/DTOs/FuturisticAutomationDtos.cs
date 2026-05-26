namespace BarberSync.Application.DTOs;

public record DemandForecastPointDto(DateTime HourUtc, string ServiceName, int ExpectedAppointments, decimal OccupancyRate, int ExpectedStockUsage);

public record FuturisticProfessionalPerformanceDto(Guid ProfessionalId, string ProfessionalName, decimal PrecisionScore, decimal EfficiencyScore, decimal SatisfactionScore, decimal UpsellRate);

public record SmartNotificationEventDto(Guid Id, string TriggerType, string Channel, string Audience, string Message, DateTime ScheduledAtUtc, bool RequiresConfirmation);

public record FuturisticOperationsSnapshotDto(
    DateTime GeneratedAtUtc,
    IReadOnlyCollection<DemandForecastPointDto> DemandForecast,
    IReadOnlyCollection<FuturisticProfessionalPerformanceDto> ProfessionalMetrics,
    IReadOnlyCollection<SmartNotificationEventDto> NotificationQueue,
    IReadOnlyDictionary<string, decimal> Kpis);
