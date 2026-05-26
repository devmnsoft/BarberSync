using BarberSync.Api.Models.Configuration;

namespace BarberSync.Api.Services.Configuration;

public record PublicBrandingDto(string TenantSlug, string CompanyName, string LogoUrl, string PrimaryColor, string SecondaryColor);
public record PublicLandingDto(string TenantSlug, string Headline, string Description, bool BookingEnabled);
public record PublicServiceDto(Guid Id, string Name, decimal Price, bool IsPublic);
public record PublicProfessionalDto(Guid Id, string Name, string Role, bool IsPublic);
public record KioskConfigDto(string DeviceCode, string TenantSlug, string BranchSlug, bool AccessibilityMode, int InactivityTimeoutSeconds);

public interface IConfigurationService
{
    PublicBrandingDto GetBranding(string tenantSlug);
    PublicLandingDto GetLanding(string tenantSlug);
    IReadOnlyList<PublicServiceDto> GetServices(string tenantSlug);
    IReadOnlyList<PublicProfessionalDto> GetProfessionals(string tenantSlug);
    KioskConfigDto GetKioskByDevice(string deviceCode);
}

public class ConfigurationService : IConfigurationService
{
    public PublicBrandingDto GetBranding(string tenantSlug) => new(tenantSlug, "Barbearia Elite", "/images/logo.png", "#111827", "#f59e0b");
    public PublicLandingDto GetLanding(string tenantSlug) => new(tenantSlug, "Seu estilo começa aqui", "Agende em poucos passos.", true);
    public IReadOnlyList<PublicServiceDto> GetServices(string tenantSlug) =>
    [
        new(Guid.NewGuid(), "Corte Masculino", 45m, true),
        new(Guid.NewGuid(), "Barba Premium", 35m, true)
    ];

    public IReadOnlyList<PublicProfessionalDto> GetProfessionals(string tenantSlug) =>
    [
        new(Guid.NewGuid(), "Lucas", "Barbeiro", true),
        new(Guid.NewGuid(), "Rafael", "Especialista em barba", true)
    ];

    public KioskConfigDto GetKioskByDevice(string deviceCode) => new(deviceCode, "barbearia-elite-demo", "centro", true, 90);
}
