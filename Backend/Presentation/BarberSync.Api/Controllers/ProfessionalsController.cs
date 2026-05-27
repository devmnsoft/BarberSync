using BarberSync.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/professionals")]
public class ProfessionalsController : ControllerBase
{
    private static readonly List<ProfessionalDto> Professionals =
    [
        new(Guid.Parse("a1111111-1111-1111-1111-111111111111"), "Rafael Barber", "Corte e Barba", "+55 11 98888-1001", "rafael@barbersync.com", "Active", 4.9m, 18450m, 8, ["Corte Degradê", "Barba Premium", "Pigmentação"]),
        new(Guid.Parse("a2222222-2222-2222-2222-222222222222"), "Lucas Navalha", "Navalhado e Fade", "+55 11 98888-1002", "lucas@barbersync.com", "Active", 4.8m, 16780m, 7, ["Corte Navalhado", "Sobrancelha", "Barba Express"]),
        new(Guid.Parse("a3333333-3333-3333-3333-333333333333"), "Bruno Estilo", "Colorimetria", "+55 11 98888-1003", "bruno@barbersync.com", "Busy", 4.7m, 15990m, 6, ["Luzes", "Platinado", "Hidratação"]),
        new(Guid.Parse("a4444444-4444-4444-4444-444444444444"), "Camila Beauty", "Estética Facial", "+55 11 98888-1004", "camila@barbersync.com", "Active", 5.0m, 14200m, 5, ["Limpeza de Pele", "Design de Sobrancelhas", "Skin Care"]),
        new(Guid.Parse("a5555555-5555-5555-5555-555555555555"), "Amanda Nails", "Nail Designer", "+55 11 98888-1005", "amanda@barbersync.com", "Inactive", 4.6m, 9380m, 0, ["Manicure", "Pedicure", "Spa dos Pés"])
    ];

    [HttpGet]
    public IActionResult Get() => Ok(ApiResponse<IEnumerable<ProfessionalDto>>.Ok(Professionals, "Profissionais carregados com sucesso.", HttpContext.TraceIdentifier));

    [HttpGet("{id:guid}")]
    public IActionResult GetById([FromRoute] Guid id)
    {
        var professional = Professionals.FirstOrDefault(p => p.Id == id);
        return professional is null
            ? NotFound(ApiResponse<object>.Fail("Profissional não encontrado.", ["ID informado não existe."], HttpContext.TraceIdentifier))
            : Ok(ApiResponse<ProfessionalDto>.Ok(professional, "Profissional carregado com sucesso.", HttpContext.TraceIdentifier));
    }

    [HttpPost]
    public IActionResult Create([FromBody] ProfessionalUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse<object>.Fail("Nome é obrigatório.", ["Informe o nome do profissional."], HttpContext.TraceIdentifier));

        var professional = new ProfessionalDto(Guid.NewGuid(), request.Name.Trim(), request.Specialty?.Trim() ?? "Generalista", request.Phone?.Trim() ?? string.Empty, request.Email?.Trim() ?? string.Empty, request.Status?.Trim() ?? "Active", request.Rating ?? 4.5m, request.MonthlyRevenue ?? 0m, request.AppointmentsToday ?? 0, request.Services ?? []);
        Professionals.Add(professional);

        return Ok(ApiResponse<ProfessionalDto>.Ok(professional, "Profissional criado com sucesso.", HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update([FromRoute] Guid id, [FromBody] ProfessionalUpsertRequest request)
    {
        var current = Professionals.FirstOrDefault(p => p.Id == id);
        if (current is null)
            return NotFound(ApiResponse<object>.Fail("Profissional não encontrado.", ["ID informado não existe."], HttpContext.TraceIdentifier));

        var updated = current with
        {
            Name = string.IsNullOrWhiteSpace(request.Name) ? current.Name : request.Name.Trim(),
            Specialty = string.IsNullOrWhiteSpace(request.Specialty) ? current.Specialty : request.Specialty.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? current.Phone : request.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? current.Email : request.Email.Trim(),
            Status = string.IsNullOrWhiteSpace(request.Status) ? current.Status : request.Status.Trim(),
            Rating = request.Rating ?? current.Rating,
            MonthlyRevenue = request.MonthlyRevenue ?? current.MonthlyRevenue,
            AppointmentsToday = request.AppointmentsToday ?? current.AppointmentsToday,
            Services = request.Services ?? current.Services
        };

        Professionals[Professionals.FindIndex(p => p.Id == id)] = updated;
        return Ok(ApiResponse<ProfessionalDto>.Ok(updated, "Profissional atualizado com sucesso.", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete([FromRoute] Guid id)
    {
        var removed = Professionals.RemoveAll(p => p.Id == id) > 0;
        return removed
            ? Ok(ApiResponse<object>.Ok(new { id }, "Profissional removido com sucesso.", HttpContext.TraceIdentifier))
            : NotFound(ApiResponse<object>.Fail("Profissional não encontrado.", ["ID informado não existe."], HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:guid}/services")]
    public IActionResult GetServices([FromRoute] Guid id)
    {
        var professional = Professionals.FirstOrDefault(p => p.Id == id);
        return professional is null
            ? NotFound(ApiResponse<object>.Fail("Profissional não encontrado.", ["ID informado não existe."], HttpContext.TraceIdentifier))
            : Ok(ApiResponse<IEnumerable<string>>.Ok(professional.Services, "Serviços carregados com sucesso.", HttpContext.TraceIdentifier));
    }

    public sealed record ProfessionalUpsertRequest(string Name, string? Specialty, string? Phone, string? Email, string? Status, decimal? Rating, decimal? MonthlyRevenue, int? AppointmentsToday, IReadOnlyList<string>? Services);
    public sealed record ProfessionalDto(Guid Id, string Name, string Specialty, string Phone, string Email, string Status, decimal Rating, decimal MonthlyRevenue, int AppointmentsToday, IReadOnlyList<string> Services);
}
