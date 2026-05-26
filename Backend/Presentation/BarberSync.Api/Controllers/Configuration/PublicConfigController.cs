using BarberSync.Api.Models.Configuration;
using BarberSync.Api.Services.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers.Configuration;

[ApiController]
[Route("api/public-config")]
public class PublicConfigController(IConfigurationService configurationService, ILogger<PublicConfigController> logger) : ControllerBase
{
    [HttpGet("{tenantSlug}")]
    public ActionResult<ApiResponse<object>> GetAll(string tenantSlug)
    {
        try
        {
            var response = new
            {
                Branding = configurationService.GetBranding(tenantSlug),
                Landing = configurationService.GetLanding(tenantSlug),
                Services = configurationService.GetServices(tenantSlug),
                Professionals = configurationService.GetProfessionals(tenantSlug)
            };
            return Ok(ApiResponse<object>.Ok(response));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao carregar configuração pública.");
            return BadRequest(ApiResponse<object>.Fail("Não foi possível carregar esta página."));
        }
    }

    [HttpGet("{tenantSlug}/branding")]
    public ActionResult<ApiResponse<PublicBrandingDto>> Branding(string tenantSlug) => Ok(ApiResponse<PublicBrandingDto>.Ok(configurationService.GetBranding(tenantSlug)));
    [HttpGet("{tenantSlug}/landing")]
    public ActionResult<ApiResponse<PublicLandingDto>> Landing(string tenantSlug) => Ok(ApiResponse<PublicLandingDto>.Ok(configurationService.GetLanding(tenantSlug)));
    [HttpGet("{tenantSlug}/services")]
    public ActionResult<ApiResponse<IReadOnlyList<PublicServiceDto>>> Services(string tenantSlug) => Ok(ApiResponse<IReadOnlyList<PublicServiceDto>>.Ok(configurationService.GetServices(tenantSlug)));
    [HttpGet("{tenantSlug}/professionals")]
    public ActionResult<ApiResponse<IReadOnlyList<PublicProfessionalDto>>> Professionals(string tenantSlug) => Ok(ApiResponse<IReadOnlyList<PublicProfessionalDto>>.Ok(configurationService.GetProfessionals(tenantSlug)));
    [HttpGet("{tenantSlug}/booking-settings")]
    public ActionResult<ApiResponse<object>> BookingSettings(string tenantSlug) => Ok(ApiResponse<object>.Ok(new { TenantSlug = tenantSlug, Enabled = true, Message = "Entraremos em contato para confirmação." }));
}
