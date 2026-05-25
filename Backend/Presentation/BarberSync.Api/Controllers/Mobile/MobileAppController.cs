using System.Diagnostics;
using BarberSync.Api.Models.Mobile;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers.Mobile;

[ApiController]
[Route("api/mobile")]
[Tags("Mobile")]
public class MobileAppController(ILogger<MobileAppController> logger) : ControllerBase
{
    private static readonly List<MobilePlantaoDto> Plantoes =
    [
        new() { Id = Guid.NewGuid(), Titulo = "Corte Premium", Status = "Disponível", Valor = 180 },
        new() { Id = Guid.NewGuid(), Titulo = "Barba Completa", Status = "Disponível", Valor = 120 }
    ];

    [HttpPost("auth/login")]
    public ActionResult<ApiResponse<MobileLoginResponseDto>> Login([FromBody] MobileLoginRequestDto request)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            logger.LogInformation("Iniciando login mobile para usuario");
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                logger.LogWarning("Login mobile bloqueado por validacao.");
                return BadRequest(new ApiResponse<MobileLoginResponseDto> { Success = false, Message = "Email e senha são obrigatórios." });
            }

            var data = new MobileLoginResponseDto { AccessToken = "mobile-demo-token", ExpiresAtUtc = DateTime.UtcNow.AddHours(8) };
            logger.LogInformation("Login mobile concluido com sucesso em {ElapsedMs}ms.", sw.ElapsedMilliseconds);
            return Ok(new ApiResponse<MobileLoginResponseDto> { Success = true, Message = "Login realizado com sucesso.", Data = data });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado no login mobile apos {ElapsedMs}ms.", sw.ElapsedMilliseconds);
            return StatusCode(500, new ApiResponse<MobileLoginResponseDto> { Success = false, Message = "Não foi possível processar o login agora." });
        }
    }

    [HttpGet("dashboard")]
    public ActionResult<ApiResponse<MobileDashboardDto>> Dashboard() => Ok(new ApiResponse<MobileDashboardDto>
    {
        Success = true,
        Message = "Dashboard carregado.",
        Data = new MobileDashboardDto { ProximosAgendamentos = 4, ConvitesPendentes = 2, PagamentosPendentes = 560 }
    });

    [HttpGet("plantoes-disponiveis")]
    public ActionResult<ApiResponse<MobilePagedResult<MobilePlantaoDto>>> PlantoesDisponiveis([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var items = Plantoes.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Ok(new ApiResponse<MobilePagedResult<MobilePlantaoDto>>
        {
            Success = true,
            Message = "Plantões disponíveis carregados.",
            Data = new MobilePagedResult<MobilePlantaoDto> { Page = page, PageSize = pageSize, Total = Plantoes.Count, Items = items }
        });
    }

    [HttpGet("convites")]
    public ActionResult<ApiResponse<List<MobileConviteDto>>> Convites() => Ok(new ApiResponse<List<MobileConviteDto>> { Success = true, Message = "Convites carregados.", Data = [new() { Id = Guid.NewGuid(), Origem = "Unidade Centro", Status = "Pendente" }] });

    [HttpGet("meus-pagamentos")]
    public ActionResult<ApiResponse<List<MobilePagamentoDto>>> Pagamentos() => Ok(new ApiResponse<List<MobilePagamentoDto>> { Success = true, Message = "Pagamentos carregados.", Data = [new() { Id = Guid.NewGuid(), Valor = 320.55m, Status = "Pendente", Competencia = DateTime.UtcNow.Date }] });

    [HttpGet("notificacoes")]
    public ActionResult<ApiResponse<List<MobileNotificacaoDto>>> Notificacoes() => Ok(new ApiResponse<List<MobileNotificacaoDto>> { Success = true, Message = "Notificações carregadas.", Data = [new() { Id = Guid.NewGuid(), Titulo = "Novo convite disponível", Lida = false }] });
}
