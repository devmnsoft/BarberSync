using BarberSync.Api.Models.Configuration;
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
        try
        {
            var resolved = string.IsNullOrWhiteSpace(deviceCode) ? "KIOSK-DEMO-001" : deviceCode.Trim();
            logger.LogInformation("Kiosk services requested for deviceCode {DeviceCode}", resolved);

            var data = DemoServices();
            var msg = "Serviços carregados com sucesso.";
            return Ok(ApiResponse<IReadOnlyList<KioskServiceDto>>.Ok(data, msg));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao carregar serviços kiosk");
            return Ok(ApiResponse<IReadOnlyList<KioskServiceDto>>.Ok(DemoServices(), "Modo demonstração ativo."));
        }
    }

    [HttpGet("professionals")]
    public ActionResult<ApiResponse<IReadOnlyList<KioskProfessionalDto>>> Professionals([FromQuery] Guid? serviceId, [FromQuery] string? deviceCode)
    {
        try
        {
            logger.LogInformation("Kiosk professionals requested for service {ServiceId} and device {DeviceCode}", serviceId, deviceCode);
            return Ok(ApiResponse<IReadOnlyList<KioskProfessionalDto>>.Ok(DemoProfessionals(), "Profissionais carregados com sucesso."));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao carregar profissionais kiosk");
            return Ok(ApiResponse<IReadOnlyList<KioskProfessionalDto>>.Ok(DemoProfessionals(), "Modo demonstração ativo."));
        }
    }

    private static List<KioskServiceDto> DemoServices() => [
        new(Guid.NewGuid(),"Corte Masculino","Barbearia","Corte moderno com acabamento profissional.",45,40,"✂️",true),
        new(Guid.NewGuid(),"Barba Tradicional","Barbearia","Barba alinhada com toalha quente e navalha.",35,30,"🪒",true),
        new(Guid.NewGuid(),"Corte + Barba","Combo","Experiência completa de corte e barba.",75,70,"💈",true),
        new(Guid.NewGuid(),"Sobrancelha","Estética","Design e alinhamento de sobrancelha.",20,15,"👁️",true),
        new(Guid.NewGuid(),"Hidratação Capilar","Estética","Tratamento capilar profissional.",60,45,"💧",true),
        new(Guid.NewGuid(),"Manicure","Beleza","Cuidado completo para as unhas.",40,50,"💅",true)
    ];
    private static List<KioskProfessionalDto> DemoProfessionals() => [new("Rafael Barber"),new("Lucas Navalha"),new("Bruno Estilo"),new("Camila Beauty"),new("Amanda Nails")];
    public sealed record KioskServiceDto(Guid Id,string Name,string Category,string Description,decimal Price,int DurationMinutes,string Icon,bool IsAvailable);
    public sealed record KioskProfessionalDto(string Name);
}
