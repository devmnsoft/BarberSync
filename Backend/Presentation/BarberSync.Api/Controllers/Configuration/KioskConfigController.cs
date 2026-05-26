using BarberSync.Api.Models.Configuration;
using BarberSync.Api.Services.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers.Configuration;

[ApiController]
[Route("api/kiosk")]
public class KioskConfigController(IConfigurationService configurationService, ILogger<KioskConfigController> logger) : ControllerBase
{
    [HttpGet("config/{deviceCode}")]
    public ActionResult<ApiResponse<KioskConfigDto>> GetByDevice(string deviceCode)
    {
        try { return Ok(ApiResponse<KioskConfigDto>.Ok(configurationService.GetKioskByDevice(deviceCode))); }
        catch (Exception ex) { logger.LogError(ex, "Erro kiosk"); return BadRequest(ApiResponse<KioskConfigDto>.Fail("Este totem ainda não foi configurado.")); }
    }

    [HttpGet("config/{tenantSlug}/{branchSlug}")]
    public ActionResult<ApiResponse<object>> GetByTenantBranch(string tenantSlug, string branchSlug) => Ok(ApiResponse<object>.Ok(new { tenantSlug, branchSlug, status = "ACTIVE" }));
    [HttpPost("register-device")]
    public ActionResult<ApiResponse<object>> RegisterDevice([FromBody] object payload) => Ok(ApiResponse<object>.Ok(payload, "Totem cadastrado com sucesso."));
    [HttpPost("heartbeat")]
    public ActionResult<ApiResponse<object>> Heartbeat([FromBody] object payload) => Ok(ApiResponse<object>.Ok(payload, "Heartbeat registrado."));
    [HttpGet("services")]
    public ActionResult<ApiResponse<IReadOnlyList<PublicServiceDto>>> Services([FromQuery] string tenantSlug) => Ok(ApiResponse<IReadOnlyList<PublicServiceDto>>.Ok(configurationService.GetServices(tenantSlug)));
    [HttpGet("payment-settings")]
    public ActionResult<ApiResponse<object>> PaymentSettings() => Ok(ApiResponse<object>.Ok(new { Pix = true, Card = true, Cash = true }));
    [HttpGet("messages")]
    public ActionResult<ApiResponse<object>> Messages() => Ok(ApiResponse<object>.Ok(new { Welcome = "Escolha seu serviço.", Confirm = "Confirme seu atendimento." }));
}
