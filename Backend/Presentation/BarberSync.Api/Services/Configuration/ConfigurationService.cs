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
        new(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Corte Masculino", 45m, true),
        new(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Barba Tradicional", 35m, true),
        new(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Corte + Barba", 75m, true),
        new(Guid.Parse("44444444-4444-4444-4444-444444444444"), "Sobrancelha", 25m, true),
        new(Guid.Parse("55555555-5555-5555-5555-555555555555"), "Hidratação Capilar", 60m, true),
        new(Guid.Parse("66666666-6666-6666-6666-666666666666"), "Manicure", 40m, true)
    ];

    public IReadOnlyList<PublicProfessionalDto> GetProfessionals(string tenantSlug) =>
    [
        new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Rafael Barber", "Fade e navalha", true),
        new(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Lucas Navalha", "Barba clássica", true),
        new(Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), "Bruno Estilo", "Corte social", true),
        new(Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), "Camila Beauty", "Estética", true),
        new(Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"), "Amanda Nails", "Manicure", true)
    ];

    public KioskConfigDto GetKioskByDevice(string deviceCode) => new(deviceCode, "barbearia-elite-demo", "centro", true, 90);
}
