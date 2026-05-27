using BarberSync.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/professionals")]
public class ProfessionalsController(ILogger<ProfessionalsController> logger) : ControllerBase
{
    private static readonly List<ProfessionalDto> Professionals =
    [
        new(Guid.Parse("a1111111-1111-1111-1111-111111111111"), "Rafael Barber", "Corte e Barba", "+55 11 98888-1001", "rafael@barbersync.com", "Active", 4.9m, 18450m, 8, ["Corte Degradê", "Barba Premium", "Pigmentação"]),
        new(Guid.Parse("a2222222-2222-2222-2222-222222222222"), "Lucas Navalha", "Navalhado e Fade", "+55 11 98888-1002", "lucas@barbersync.com", "Active", 4.8m, 16780m, 7, ["Corte Navalhado", "Sobrancelha", "Barba Express"]),
        new(Guid.Parse("a3333333-3333-3333-3333-333333333333"), "Bruno Estilo", "Colorimetria", "+55 11 98888-1003", "bruno@barbersync.com", "Busy", 4.7m, 15990m, 6, ["Luzes", "Platinado", "Hidratação"]),
        new(Guid.Parse("a4444444-4444-4444-4444-444444444444"), "Camila Beauty", "Estética Facial", "+55 11 98888-1004", "camila@barbersync.com", "Active", 5.0m, 14200m, 5, ["Limpeza de Pele", "Design de Sobrancelhas", "Skin Care"]),
        new(Guid.Parse("a5555555-5555-5555-5555-555555555555"), "Amanda Nails", "Nail Designer", "+55 11 98888-1005", "amanda@barbersync.com", "Active", 4.6m, 11380m, 4, ["Manicure", "Pedicure", "Spa dos Pés"]),
        new(Guid.Parse("a6666666-6666-6666-6666-666666666666"), "Felipe Corte Fino", "Barbearia Clássica", "+55 11 98888-1006", "felipe@barbersync.com", "Active", 4.8m, 17650m, 7, ["Corte Clássico", "Barba Tradicional"]),
        new(Guid.Parse("a7777777-7777-7777-7777-777777777777"), "Amanda Nails", "Nail Art", "+55 11 98888-1007", "amanda.nails2@barbersync.com", "Busy", 4.7m, 12040m, 5, ["Nail Art", "Esmaltação em Gel"]),
        new(Guid.Parse("a8888888-8888-8888-8888-888888888888"), "Priscila Beauty", "Cabelo e Make", "+55 11 98888-1008", "priscila@barbersync.com", "Active", 4.9m, 19120m, 8, ["Escova", "Maquiagem", "Penteado"])
    ];

    [HttpGet]
    public IActionResult Get() { try { return Ok(ApiResponse<IEnumerable<ProfessionalDto>>.Ok(Professionals, "Profissionais carregados com sucesso.", HttpContext.TraceIdentifier)); } catch (Exception ex) { logger.LogError(ex, "Erro ao listar profissionais."); return StatusCode(500, ApiResponse<object>.Fail("Erro ao listar profissionais.", [ex.Message], HttpContext.TraceIdentifier)); } }

    [HttpGet("{id:guid}")]
    public IActionResult GetById([FromRoute] Guid id) { try { var professional = Professionals.FirstOrDefault(p => p.Id == id); return professional is null ? NotFound(ApiResponse<object>.Fail("Profissional não encontrado.", ["ID informado não existe."], HttpContext.TraceIdentifier)) : Ok(ApiResponse<ProfessionalDto>.Ok(professional, "Profissional carregado com sucesso.", HttpContext.TraceIdentifier)); } catch (Exception ex) { logger.LogError(ex, "Erro ao buscar profissional por id: {Id}", id); return StatusCode(500, ApiResponse<object>.Fail("Erro ao buscar profissional.", [ex.Message], HttpContext.TraceIdentifier)); } }

    [HttpPost]
    public IActionResult Create([FromBody] ProfessionalUpsertRequest request) { try { if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(ApiResponse<object>.Fail("Nome é obrigatório.", ["Informe o nome do profissional."], HttpContext.TraceIdentifier)); var professional = new ProfessionalDto(Guid.NewGuid(), request.Name.Trim(), request.Specialty?.Trim() ?? "Generalista", request.Phone?.Trim() ?? string.Empty, request.Email?.Trim() ?? string.Empty, request.Status?.Trim() ?? "Active", request.Rating ?? 4.5m, request.MonthlyRevenue ?? 0m, request.AppointmentsToday ?? 0, request.Services ?? []); Professionals.Add(professional); return Ok(ApiResponse<ProfessionalDto>.Ok(professional, "Profissional criado com sucesso.", HttpContext.TraceIdentifier)); } catch (Exception ex) { logger.LogError(ex, "Erro ao criar profissional"); return StatusCode(500, ApiResponse<object>.Fail("Erro ao criar profissional.", [ex.Message], HttpContext.TraceIdentifier)); } }

    [HttpPut("{id:guid}")]
    public IActionResult Update([FromRoute] Guid id, [FromBody] ProfessionalUpsertRequest request) { try { var current = Professionals.FirstOrDefault(p => p.Id == id); if (current is null) return NotFound(ApiResponse<object>.Fail("Profissional não encontrado.", ["ID informado não existe."], HttpContext.TraceIdentifier)); var updated = current with { Name = string.IsNullOrWhiteSpace(request.Name) ? current.Name : request.Name.Trim(), Specialty = string.IsNullOrWhiteSpace(request.Specialty) ? current.Specialty : request.Specialty.Trim(), Phone = string.IsNullOrWhiteSpace(request.Phone) ? current.Phone : request.Phone.Trim(), Email = string.IsNullOrWhiteSpace(request.Email) ? current.Email : request.Email.Trim(), Status = string.IsNullOrWhiteSpace(request.Status) ? current.Status : request.Status.Trim(), Rating = request.Rating ?? current.Rating, MonthlyRevenue = request.MonthlyRevenue ?? current.MonthlyRevenue, AppointmentsToday = request.AppointmentsToday ?? current.AppointmentsToday, Services = request.Services ?? current.Services }; Professionals[Professionals.FindIndex(p => p.Id == id)] = updated; return Ok(ApiResponse<ProfessionalDto>.Ok(updated, "Profissional atualizado com sucesso.", HttpContext.TraceIdentifier)); } catch (Exception ex) { logger.LogError(ex, "Erro ao atualizar profissional {Id}", id); return StatusCode(500, ApiResponse<object>.Fail("Erro ao atualizar profissional.", [ex.Message], HttpContext.TraceIdentifier)); } }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete([FromRoute] Guid id) { try { var removed = Professionals.RemoveAll(p => p.Id == id) > 0; return removed ? Ok(ApiResponse<object>.Ok(new { id }, "Profissional removido com sucesso.", HttpContext.TraceIdentifier)) : NotFound(ApiResponse<object>.Fail("Profissional não encontrado.", ["ID informado não existe."], HttpContext.TraceIdentifier)); } catch (Exception ex) { logger.LogError(ex, "Erro ao remover profissional {Id}", id); return StatusCode(500, ApiResponse<object>.Fail("Erro ao remover profissional.", [ex.Message], HttpContext.TraceIdentifier)); } }

    [HttpGet("{id:guid}/services")]
    public IActionResult GetServices([FromRoute] Guid id) { try { var professional = Professionals.FirstOrDefault(p => p.Id == id); return professional is null ? NotFound(ApiResponse<object>.Fail("Profissional não encontrado.", ["ID informado não existe."], HttpContext.TraceIdentifier)) : Ok(ApiResponse<IEnumerable<string>>.Ok(professional.Services, "Serviços carregados com sucesso.", HttpContext.TraceIdentifier)); } catch (Exception ex) { logger.LogError(ex, "Erro ao obter serviços do profissional {Id}", id); return StatusCode(500, ApiResponse<object>.Fail("Erro ao obter serviços do profissional.", [ex.Message], HttpContext.TraceIdentifier)); } }

    public sealed record ProfessionalUpsertRequest(string Name, string? Specialty, string? Phone, string? Email, string? Status, decimal? Rating, decimal? MonthlyRevenue, int? AppointmentsToday, IReadOnlyList<string>? Services);
    public sealed record ProfessionalDto(Guid Id, string Name, string Specialty, string Phone, string Email, string Status, decimal Rating, decimal MonthlyRevenue, int AppointmentsToday, IReadOnlyList<string> Services);
}
