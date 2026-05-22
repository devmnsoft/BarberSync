namespace BarberSync.Application.DTOs;

public record SubscriptionPlanDto(Guid Id, string Code, string Name, int MaxUsers, int MaxProfessionals, int MaxBranches, int MaxMonthlyAppointments, bool AiEnabled, bool TotemEnabled, bool AdvancedReports, bool WhatsappNotifications, bool BiIntegration, decimal Price);
public record SubscriptionDto(Guid Id, Guid TenantId, Guid PlanId, string Status, DateTime StartedAt, DateTime? EndsAt);
public record TenantUsageDto(Guid TenantId, int Users, int Professionals, int Branches, int MonthlyAppointments, int AiRequests, int TotemSessions);
public record InvoiceDto(Guid Id, Guid TenantId, decimal Amount, string Status, DateTime DueDate);
public record CompanyDto(Guid Id, Guid TenantId, string Name, string Slug, bool IsBlocked);
public record BranchDto(Guid Id, Guid TenantId, Guid CompanyId, string Name);
public record TenantSettingsDto(Guid TenantId, string Timezone, string Currency, int CancellationWindowHours, decimal CashbackPercent, decimal CommissionPercent);
public record CompanyBrandingDto(Guid TenantId, string Name, string LogoUrl, string PrimaryColor, string SecondaryColor, string Slogan);
public record OnboardingStateDto(Guid TenantId, string Step, CompanyDto? Company, SubscriptionDto? Subscription, CompanyBrandingDto? Branding);
