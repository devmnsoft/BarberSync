using BarberSync.Application.DTOs;
using BarberSync.Application.Services.Saas;

namespace BarberSync.Tests;

public class CommercialPlatformTests
{
    [Fact]
    public void WhiteLabelServiceTests_Should_Upsert_Branding()
    {
        var store = new InMemorySaasStore();
        var service = new CommercialPlatformService(store);
        var tenantId = Guid.NewGuid();
        var branding = new TenantBrandingDto(tenantId, "Teste", "logo", "slogan", "#111", "#222", "dark", "cover", "app", "welcome", "book");
        service.UpsertBranding(branding);
        Assert.Equal("Teste", service.GetBranding(tenantId)?.CommercialName);
    }

    [Fact]
    public void PublicSchedulingServiceTests_Should_Create_Appointment()
    {
        var service = new CommercialPlatformService(new InMemorySaasStore());
        var appointment = service.CreatePublicAppointment("barbeariaelite", new PublicAppointmentRequestDto("Ana", "119999", "ana@email.com", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(1), null));
        Assert.Equal("CONFIRMED", appointment.Status);
    }

    [Fact]
    public void MarketplaceServiceTests_Should_Return_Profiles() =>
        Assert.NotEmpty(new CommercialPlatformService(new InMemorySaasStore()).SearchMarketplace("São Paulo", null));

    [Fact]
    public void BillingServiceTests_Should_Start_Trial() =>
        Assert.Equal("TRIAL", new CommercialPlatformService(new InMemorySaasStore()).StartTrial(Guid.NewGuid(), "STARTER").BillingPeriod);

    [Fact]
    public void SupportServiceTests_Should_Open_Ticket() =>
        Assert.Equal("OPEN", new CommercialPlatformService(new InMemorySaasStore()).OpenTicket(new SupportTicketDto(Guid.NewGuid(), Guid.NewGuid(), "Ajuda", "billing", "high", "OPEN", DateTime.UtcNow)).Status);

    [Fact]
    public void ProductAnalyticsServiceTests_Should_Track_Event() =>
        Assert.Equal("login", new CommercialPlatformService(new InMemorySaasStore()).TrackEvent(new ProductEventDto(Guid.NewGuid(), Guid.NewGuid(), "login", DateTime.UtcNow, null)).EventName);

    [Fact]
    public void ExportJobServiceTests_Should_Request_Export() =>
        Assert.Equal("PROCESSING", new CommercialPlatformService(new InMemorySaasStore()).RequestExport(Guid.NewGuid(), "appointments").Status);
}
