namespace BarberSync.Application.DTOs;

public record DashboardSummaryDto(decimal RevenueToday, decimal RevenueMonth, decimal AvgTicket, int ServicesDone, int TodayAppointments, int FutureAppointments, int NewClients, int ReturningClients, int ProfessionalsBusy, int ScheduleOccupancyPercent, int CriticalStockItems, decimal CashbackGenerated, decimal PendingCommissions, int AiDetectedServices, decimal CancellationRate, string PeakHours);
public record ReviewDto(Guid Id, Guid TenantId, Guid AppointmentId, int Rating, string Comment, int ProfessionalRating, int ServiceRating, DateTime CreatedAt);
public record NpsResponseDto(Guid Id, Guid TenantId, int Score, string Comment, DateTime CreatedAt);
public record SatisfactionReportDto(decimal AvgRating, decimal NpsScore, IReadOnlyList<string> TopProfessionals, IReadOnlyList<string> TopServices, IReadOnlyList<string> RecentComplaints);
