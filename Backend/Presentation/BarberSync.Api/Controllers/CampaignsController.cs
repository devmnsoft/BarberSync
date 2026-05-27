using BarberSync.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/campaigns")]
public class CampaignsController : ControllerBase
{
    private static readonly List<CampaignDto> Campaigns =
    [
        new(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Reativação 30 dias", "Retorno", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)), "Ativa", 124),
        new(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Cashback Semana", "Cashback", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)), "Ativa", 89)
    ];

    [HttpGet]
    public IActionResult Get() => Ok(ApiResponse<IEnumerable<CampaignDto>>.Ok(Campaigns, "Campanhas carregadas com sucesso.", HttpContext.TraceIdentifier));



    [HttpGet("{id:guid}")]
    public IActionResult GetById([FromRoute] Guid id)
    {
        var campaign = Campaigns.FirstOrDefault(x => x.Id == id);
        return campaign is null
            ? NotFound(ApiResponse<object>.Fail("Campanha não encontrada.", ["ID informado não existe."], HttpContext.TraceIdentifier))
            : Ok(ApiResponse<CampaignDto>.Ok(campaign, "Campanha carregada com sucesso.", HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:guid}")]
    public IActionResult Update([FromRoute] Guid id, [FromBody] CreateCampaignRequest request)
    {
        var current = Campaigns.FirstOrDefault(x => x.Id == id);
        if (current is null)
            return NotFound(ApiResponse<object>.Fail("Campanha não encontrada.", ["ID informado não existe."], HttpContext.TraceIdentifier));

        var updated = current with
        {
            Name = string.IsNullOrWhiteSpace(request.Name) ? current.Name : request.Name.Trim(),
            Type = string.IsNullOrWhiteSpace(request.Type) ? current.Type : request.Type.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };

        Campaigns[Campaigns.FindIndex(x => x.Id == id)] = updated;
        return Ok(ApiResponse<CampaignDto>.Ok(updated, "Campanha atualizada com sucesso.", HttpContext.TraceIdentifier));
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete([FromRoute] Guid id)
    {
        var removed = Campaigns.RemoveAll(x => x.Id == id) > 0;
        return removed
            ? Ok(ApiResponse<object>.Ok(new { id }, "Campanha removida com sucesso.", HttpContext.TraceIdentifier))
            : NotFound(ApiResponse<object>.Fail("Campanha não encontrada.", ["ID informado não existe."], HttpContext.TraceIdentifier));
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateCampaignRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(ApiResponse<object>.Fail("Nome da campanha é obrigatório.", ["Informe o nome da campanha."], HttpContext.TraceIdentifier));

        var campaign = new CampaignDto(Guid.NewGuid(), request.Name.Trim(), request.Type?.Trim() ?? "Desconto", request.StartDate, request.EndDate, "Ativa", 0);
        Campaigns.Add(campaign);
        return Ok(ApiResponse<CampaignDto>.Ok(campaign, "Campanha criada com sucesso.", HttpContext.TraceIdentifier));
    }

    public sealed record CreateCampaignRequest(string Name, string? Type, DateOnly StartDate, DateOnly EndDate);
    public sealed record CampaignDto(Guid Id, string Name, string Type, DateOnly StartDate, DateOnly EndDate, string Status, int Reach);
}
