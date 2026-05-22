using BarberSync.Application.DTOs;

namespace BarberSync.Application.Services.Saas;

public class CommercialPlatformService
{
    private readonly InMemorySaasStore _store;

    public CommercialPlatformService(InMemorySaasStore store)
    {
        _store = store;
        if (_store.TenantBranding.Count == 0) Seed();
    }

    public TenantBrandingDto UpsertBranding(TenantBrandingDto input)
    {
        _store.TenantBranding[input.TenantId] = input;
        return input;
    }
    public TenantBrandingDto? GetBranding(Guid tenantId) => _store.TenantBranding.GetValueOrDefault(tenantId);
    public TenantDomainDto UpsertDomain(TenantDomainDto input) { _store.TenantDomains[input.TenantId] = input; return input; }
    public TenantPublicPageDto UpsertPublicPage(TenantPublicPageDto input) { _store.TenantPublicPages[input.TenantId] = input; return input; }

    public IReadOnlyList<PublicServiceDto> GetPublicServices(string tenantSlug) => _store.PublicServices.Where(x => TenantSlug(x.TenantId) == tenantSlug).ToList();
    public IReadOnlyList<PublicProfessionalDto> GetPublicProfessionals(string tenantSlug) => _store.PublicProfessionals.Where(x => TenantSlug(x.TenantId) == tenantSlug).ToList();
    public IReadOnlyList<DateTime> GetAvailableTimes(string tenantSlug, DateOnly day)
    {
        var date = day.ToDateTime(TimeOnly.MinValue);
        return Enumerable.Range(9, 9).Select(h => date.AddHours(h)).Where(t => _store.PublicAppointments.All(a => a.TenantSlug != tenantSlug || a.StartsAt != t || a.Status == "CANCELLED")).ToList();
    }
    public PublicAppointmentDto CreatePublicAppointment(string tenantSlug, PublicAppointmentRequestDto request)
    {
        var appointment = new PublicAppointmentDto(Guid.NewGuid(), tenantSlug, request.ClientName, request.Phone, request.Email, request.ServiceId, request.ProfessionalId, request.StartsAt, "CONFIRMED", Guid.NewGuid().ToString("N"));
        _store.PublicAppointments.Add(appointment);
        return appointment;
    }

    public IReadOnlyList<MarketplaceProfileDto> SearchMarketplace(string? city, string? service) => _store.MarketplaceProfiles
        .Where(p => string.IsNullOrWhiteSpace(city) || p.City.Equals(city, StringComparison.OrdinalIgnoreCase))
        .OrderByDescending(p => p.Rating).ThenBy(p => p.MinPrice).ToList();

    public BillingCycleDto StartTrial(Guid tenantId, string planCode)
    {
        var cycle = new BillingCycleDto(Guid.NewGuid(), tenantId, planCode, "TRIAL", DateTime.UtcNow, DateTime.UtcNow.AddDays(14), "ACTIVE");
        _store.BillingCycles.Add(cycle);
        return cycle;
    }

    public SupportTicketDto OpenTicket(SupportTicketDto ticket) { _store.SupportTickets.Add(ticket); return ticket; }
    public ProductEventDto TrackEvent(ProductEventDto ev) { _store.ProductEvents.Add(ev); return ev; }
    public ExportJobDto RequestExport(Guid tenantId, string resource)
    {
        var job = new ExportJobDto(Guid.NewGuid(), tenantId, resource, "PROCESSING", DateTime.UtcNow, null);
        _store.ExportJobs.Add(job);
        return job;
    }

    private string TenantSlug(Guid tenantId) => _store.Companies.FirstOrDefault(c => c.TenantId == tenantId)?.Slug ?? "demo";
    private void Seed()
    {
        var tenantId = Guid.NewGuid();
        _store.Companies.Add(new CompanyDto(Guid.NewGuid(), tenantId, "Barbearia Elite Demo", "barbeariaelite", false));
        _store.TenantBranding[tenantId] = new TenantBrandingDto(tenantId, "Barbearia Elite", "https://picsum.photos/100", "Estilo que marca", "#0F172A", "#F59E0B", "dark", "https://picsum.photos/1200/300", "Barbearia Elite App", "Bem-vindo", "Agende seu horário");
        _store.TenantDomains[tenantId] = new TenantDomainDto(tenantId, "barbeariaelite", null, false);
        _store.TenantPublicPages[tenantId] = new TenantPublicPageDto(tenantId, "barbeariaelite", "Agende online", "Serviços premium", true, true);
        _store.PublicServices.Add(new PublicServiceDto(Guid.NewGuid(), tenantId, "Corte Tradicional", 45, 45));
        _store.PublicProfessionals.Add(new PublicProfessionalDto(Guid.NewGuid(), tenantId, "Rafael", 4.8));
        _store.MarketplaceProfiles.Add(new MarketplaceProfileDto(tenantId, "barbeariaelite", "Barbearia Elite", "São Paulo", "Moema", 4.9, 45, true));
    }
}
