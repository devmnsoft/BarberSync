namespace BarberSync.Application.DTOs;

public record TenantBrandingDto(Guid TenantId, string CommercialName, string LogoUrl, string Slogan, string PrimaryColor, string SecondaryColor, string ThemeMode, string CoverImageUrl, string AppName, string KioskWelcomeText, string PublicBookingTitle);
public record TenantDomainDto(Guid TenantId, string Subdomain, string? CustomDomain, bool Verified);
public record TenantPublicPageDto(Guid TenantId, string Slug, string Headline, string Description, bool EnableCoupons, bool EnableCashback);
public record PublicServiceDto(Guid Id, Guid TenantId, string Name, decimal Price, int DurationMinutes);
public record PublicProfessionalDto(Guid Id, Guid TenantId, string Name, double Rating);
public record PublicAppointmentRequestDto(string ClientName, string Phone, string Email, Guid ServiceId, Guid ProfessionalId, DateTime StartsAt, string? CouponCode);
public record PublicAppointmentDto(Guid Id, string TenantSlug, string ClientName, string Phone, string Email, Guid ServiceId, Guid ProfessionalId, DateTime StartsAt, string Status, string ConfirmationToken);
public record MarketplaceProfileDto(Guid TenantId, string TenantSlug, string BusinessName, string City, string District, double Rating, decimal MinPrice, bool Featured);
public record MarketplacePromotionDto(Guid Id, Guid TenantId, string Title, string Description, decimal DiscountPercent, DateTime ValidUntil);
public record BillingCycleDto(Guid Id, Guid TenantId, string PlanCode, string BillingPeriod, DateTime StartDate, DateTime EndDate, string Status);
public record SubscriptionChangeDto(Guid Id, Guid TenantId, string FromPlan, string ToPlan, DateTime ChangedAt, string ChangeType);
public record SupportTicketDto(Guid Id, Guid TenantId, string Subject, string Category, string Priority, string Status, DateTime OpenedAt);
public record ProductEventDto(Guid Id, Guid TenantId, string EventName, DateTime OccurredAt, Dictionary<string, string>? Properties);
public record ExportJobDto(Guid Id, Guid TenantId, string Resource, string Status, DateTime RequestedAt, DateTime? FinishedAt);
