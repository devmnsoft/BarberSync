using BarberSync.Api.Models.Configuration;
using BarberSync.Api.Models.Kiosk;
using BarberSync.Api.Services.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers.Configuration;

[ApiController]
[Route("api/kiosk")]
public class KioskConfigController(IConfigurationService configurationService, ILogger<KioskConfigController> logger) : ControllerBase
{
    [HttpGet("services")]
    public ActionResult<ApiResponse<IReadOnlyList<KioskServiceDto>>> Services([FromQuery] string? deviceCode)
    {
        var resolved = string.IsNullOrWhiteSpace(deviceCode) ? "KIOSK-DEMO-001" : deviceCode.Trim();
        try
        {
            var kioskConfig = configurationService.GetKioskByDevice(resolved);
            logger.LogInformation("Kiosk services requested for deviceCode {DeviceCode} tenant {TenantSlug}", resolved, kioskConfig.TenantSlug);
            var data = GetDemoKioskServices();
            return Ok(ApiResponse<IReadOnlyList<KioskServiceDto>>.Ok(data, "Serviços carregados com sucesso em modo demonstração."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao carregar serviços kiosk para {DeviceCode}", resolved);
            return Ok(ApiResponse<IReadOnlyList<KioskServiceDto>>.Ok(GetDemoKioskServices(), "Serviços carregados em modo demonstração após falha temporária."));
        }
    }

    [HttpGet("professionals")]
    public ActionResult<ApiResponse<IReadOnlyList<KioskProfessionalDto>>> Professionals([FromQuery] string? serviceId, [FromQuery] string? deviceCode)
    {
        try
        {
            var resolvedDevice = string.IsNullOrWhiteSpace(deviceCode) ? "KIOSK-DEMO-001" : deviceCode.Trim();
            var kioskConfig = configurationService.GetKioskByDevice(resolvedDevice);
            logger.LogInformation("Kiosk professionals requested for service {ServiceId} and device {DeviceCode} tenant {TenantSlug}", serviceId, resolvedDevice, kioskConfig.TenantSlug);
            return Ok(ApiResponse<IReadOnlyList<KioskProfessionalDto>>.Ok(GetDemoProfessionals(), "Profissionais carregados com sucesso em modo demonstração."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao carregar profissionais kiosk");
            return Ok(ApiResponse<IReadOnlyList<KioskProfessionalDto>>.Ok(GetDemoProfessionals(), "Modo demonstração ativo."));
        }
    }


    [HttpGet("status")]
    public IActionResult Status([FromQuery] string? deviceCode)
    {
        var resolved = string.IsNullOrWhiteSpace(deviceCode) ? "KIOSK-DEMO-001" : deviceCode.Trim();
        return Ok(ApiResponse<object>.Ok(new { online = true, deviceCode = resolved, currentStep = "Pronto para atendimento", queue = 3, lastHeartbeat = DateTime.UtcNow, paymentsMockEnabled = true, isDemo = true }, "Totem online em modo demonstração."));
    }

    [HttpPost("client/find-by-phone")]
    public IActionResult FindClientByPhone([FromBody] object payload) => Ok(ApiResponse<object>.Ok(new { name = "Cliente Demo", phone = "(11) 99999-9999", isDemo = true }, "Cliente localizado em modo demonstração."));

    [HttpPost("client/quick-register")]
    public IActionResult QuickRegister([FromBody] object payload) => Ok(ApiResponse<object>.Ok(new { protocol = $"KIOSK-CLI-{DateTime.UtcNow:HHmmss}", isDemo = true }, "Cadastro rápido realizado em modo demonstração."));

    [HttpPost("payment/mock")]
    public IActionResult MockPayment([FromBody] object payload) => Ok(ApiResponse<object>.Ok(new { status = "APPROVED", authorizationCode = $"PAY-{DateTime.UtcNow:HHmmss}", isDemo = true }, "Pagamento simulado aprovado."));

    [HttpPost("review")]
    public IActionResult Review([FromBody] object payload) => Ok(ApiResponse<object>.Ok(new { received = true, isDemo = true }, "Avaliação recebida em modo demonstração."));

    private static List<KioskServiceDto> GetDemoKioskServices() =>
    [
        new() { Id = Guid.NewGuid(), Name = "Corte Masculino", Category = "Barbearia", Description = "Corte moderno com acabamento profissional.", Price = 45.00m, DurationMinutes = 40, Icon = "✂️", IsAvailable = true, IsDemo = true },
        new() { Id = Guid.NewGuid(), Name = "Barba Tradicional", Category = "Barbearia", Description = "Barba alinhada com toalha quente e navalha.", Price = 35.00m, DurationMinutes = 30, Icon = "🪒", IsAvailable = true, IsDemo = true },
        new() { Id = Guid.NewGuid(), Name = "Corte + Barba", Category = "Combo", Description = "Experiência completa de corte e barba.", Price = 75.00m, DurationMinutes = 70, Icon = "💈", IsAvailable = true, IsDemo = true },
        new() { Id = Guid.NewGuid(), Name = "Sobrancelha", Category = "Estética", Description = "Design e alinhamento de sobrancelha.", Price = 20.00m, DurationMinutes = 15, Icon = "👁️", IsAvailable = true, IsDemo = true },
        new() { Id = Guid.NewGuid(), Name = "Hidratação Capilar", Category = "Estética", Description = "Tratamento capilar profissional.", Price = 60.00m, DurationMinutes = 45, Icon = "💧", IsAvailable = true, IsDemo = true },
        new() { Id = Guid.NewGuid(), Name = "Manicure", Category = "Beleza", Description = "Cuidado completo para as unhas.", Price = 40.00m, DurationMinutes = 50, Icon = "💅", IsAvailable = true, IsDemo = true }
    ];

    private static List<KioskProfessionalDto> GetDemoProfessionals() =>
    [
        new() { Name = "Rafael Barber", Specialty = "Fade e navalha", EstimatedWaitMinutes = 10, AvatarInitials = "RB" },
        new() { Name = "Lucas Navalha", Specialty = "Barba clássica", EstimatedWaitMinutes = 15, AvatarInitials = "LN" },
        new() { Name = "Bruno Estilo", Specialty = "Corte social", EstimatedWaitMinutes = 20, AvatarInitials = "BE" },
        new() { Name = "Camila Beauty", Specialty = "Estética", EstimatedWaitMinutes = 8, AvatarInitials = "CB" },
        new() { Name = "Amanda Nails", Specialty = "Manicure", EstimatedWaitMinutes = 12, AvatarInitials = "AN" }
    ];
}
