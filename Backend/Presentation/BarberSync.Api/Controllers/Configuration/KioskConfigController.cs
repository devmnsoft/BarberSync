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
            var data = DemoServices();
            var msg = resolved == "KIOSK-DEMO-001" ? "Serviços carregados com sucesso." : "Dispositivo demo carregado";
            return Ok(ApiResponse<IReadOnlyList<KioskServiceDto>>.Ok(data, msg));
        }
        catch (Exception ex) { logger.LogError(ex, "Erro ao carregar serviços kiosk"); return Ok(ApiResponse<IReadOnlyList<KioskServiceDto>>.Ok(DemoServices(), "Dispositivo demo carregado")); }
    }

    [HttpGet("professionals")]
    public ActionResult<ApiResponse<IReadOnlyList<KioskProfessionalDto>>> Professionals([FromQuery] Guid? serviceId, [FromQuery] string? deviceCode)
    {
        try { return Ok(ApiResponse<IReadOnlyList<KioskProfessionalDto>>.Ok(DemoProfessionals(), "Profissionais carregados com sucesso.")); }
        catch (Exception ex) { logger.LogError(ex, "Erro ao carregar profissionais kiosk"); return Ok(ApiResponse<IReadOnlyList<KioskProfessionalDto>>.Ok(DemoProfessionals(), "Dispositivo demo carregado")); }
    }

    private static List<KioskServiceDto> DemoServices() => [
        new("Corte Masculino","Barbearia",45,40,"Corte moderno com acabamento profissional."),new("Barba Tradicional","Barbearia",35,30,"Barba alinhada com toalha quente e navalha."),new("Corte + Barba","Combo",75,70,"Experiência completa de corte e barba."),new("Sobrancelha","Estética",20,15,"Design e alinhamento de sobrancelha."),new("Hidratação Capilar","Estética",60,45,"Tratamento capilar profissional."),new("Manicure","Beleza",40,50,"Cuidado completo para as unhas.")
    ];
    private static List<KioskProfessionalDto> DemoProfessionals() => [new("Rafael Barber"),new("Lucas Navalha"),new("Bruno Estilo"),new("Camila Beauty"),new("Amanda Nails")];
    public sealed record KioskServiceDto(string Name,string Category,decimal Price,int DurationMinutes,string Description);
    public sealed record KioskProfessionalDto(string Name);
}
