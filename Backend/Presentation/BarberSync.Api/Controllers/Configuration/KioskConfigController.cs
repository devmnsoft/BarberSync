using BarberSync.Api.Models.Configuration;
using BarberSync.Api.Models.Kiosk;
using BarberSync.Api.Services.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers.Configuration;

[ApiController]
[Route("api/kiosk")]
public class KioskConfigController(IConfigurationService configurationService, ILogger<KioskConfigController> logger, IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet("services")]
    public ActionResult<ApiResponse<IReadOnlyList<KioskServiceDto>>> Services([FromQuery] string? deviceCode)
    {
        var resolved = string.IsNullOrWhiteSpace(deviceCode) ? "KIOSK-DEMO-001" : deviceCode.Trim();
        try
        {
            logger.LogInformation("Kiosk services requested for deviceCode {DeviceCode}", resolved);
            var data = GetDemoKioskServices();
            return Ok(ApiResponse<IReadOnlyList<KioskServiceDto>>.Ok(data, "Serviços carregados com sucesso em modo demonstração."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao carregar serviços kiosk para {DeviceCode}", resolved);
            if (environment.IsDevelopment())
            {
                return Ok(ApiResponse<IReadOnlyList<KioskServiceDto>>.Ok(GetDemoKioskServices(), "Serviços carregados em modo demonstração após falha temporária."));
            }
            return StatusCode(500, ApiResponse<IReadOnlyList<KioskServiceDto>>.Fail("Falha ao carregar serviços do kiosk."));
        }
    }

    [HttpGet("professionals")]
    public ActionResult<ApiResponse<IReadOnlyList<KioskProfessionalDto>>> Professionals([FromQuery] Guid? serviceId, [FromQuery] string? deviceCode)
    {
        try
        {
            logger.LogInformation("Kiosk professionals requested for service {ServiceId} and device {DeviceCode}", serviceId, deviceCode);
            return Ok(ApiResponse<IReadOnlyList<KioskProfessionalDto>>.Ok(GetDemoProfessionals(), "Profissionais carregados com sucesso em modo demonstração."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao carregar profissionais kiosk");
            return Ok(ApiResponse<IReadOnlyList<KioskProfessionalDto>>.Ok(GetDemoProfessionals(), "Modo demonstração ativo."));
        }
    }

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
