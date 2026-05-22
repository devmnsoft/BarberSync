using BarberSync.Application.DTOs;
using BarberSync.Application.Services.Saas;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
public class CommercialPlatformController(CommercialPlatformService service) : ControllerBase
{
    [HttpPost("/api/white-label/branding")] public ActionResult<TenantBrandingDto> UpsertBranding([FromBody] TenantBrandingDto dto) => Ok(service.UpsertBranding(dto));
    [HttpGet("/api/white-label/branding/{tenantId:guid}")] public ActionResult<TenantBrandingDto?> GetBranding(Guid tenantId) => Ok(service.GetBranding(tenantId));
    [HttpPost("/api/white-label/domain")] public ActionResult<TenantDomainDto> UpsertDomain([FromBody] TenantDomainDto dto) => Ok(service.UpsertDomain(dto));
    [HttpPost("/api/white-label/public-page")] public ActionResult<TenantPublicPageDto> UpsertPublicPage([FromBody] TenantPublicPageDto dto) => Ok(service.UpsertPublicPage(dto));

    [HttpGet("/api/public/{tenantSlug}/services")] public ActionResult<IReadOnlyList<PublicServiceDto>> Services(string tenantSlug) => Ok(service.GetPublicServices(tenantSlug));
    [HttpGet("/api/public/{tenantSlug}/professionals")] public ActionResult<IReadOnlyList<PublicProfessionalDto>> Professionals(string tenantSlug) => Ok(service.GetPublicProfessionals(tenantSlug));
    [HttpGet("/api/public/{tenantSlug}/available-times")] public ActionResult<IReadOnlyList<DateTime>> Times(string tenantSlug, [FromQuery] DateOnly day) => Ok(service.GetAvailableTimes(tenantSlug, day));
    [HttpPost("/api/public/{tenantSlug}/appointments")] public ActionResult<PublicAppointmentDto> CreateAppointment(string tenantSlug, [FromBody] PublicAppointmentRequestDto dto) => Ok(service.CreatePublicAppointment(tenantSlug, dto));

    [HttpGet("/api/marketplace/search")] public ActionResult<IReadOnlyList<MarketplaceProfileDto>> Search([FromQuery] string? city, [FromQuery] string? serviceName) => Ok(service.SearchMarketplace(city, serviceName));

    [HttpPost("/api/billing/trial")] public ActionResult<BillingCycleDto> Trial([FromQuery] Guid tenantId, [FromQuery] string planCode = "STARTER") => Ok(service.StartTrial(tenantId, planCode));
    [HttpPost("/api/support/tickets")] public ActionResult<SupportTicketDto> Ticket([FromBody] SupportTicketDto dto) => Ok(service.OpenTicket(dto));
    [HttpPost("/api/product-analytics/events")] public ActionResult<ProductEventDto> Event([FromBody] ProductEventDto dto) => Ok(service.TrackEvent(dto));
    [HttpPost("/api/exports/request")] public ActionResult<ExportJobDto> Export([FromQuery] Guid tenantId, [FromQuery] string resource) => Ok(service.RequestExport(tenantId, resource));
}
