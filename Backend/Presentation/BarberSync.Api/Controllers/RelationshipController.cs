using System.Text.Json;
using BarberSync.Api.Security;
using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Authorize, Route("api/relationship")]
public sealed class RelationshipController(EnterpriseDataService data, ILogger<RelationshipController> logger) : EnterpriseCrudController(data, logger, "clients")
{
    [HttpGet("dashboard"), RequirePermission("Client.Read")]
    public Task<IActionResult> Dashboard(CancellationToken ct) => Safe(async () => Ok(Envelope(await data.RelationshipDashboardAsync(ct), "Indicadores de relacionamento carregados.")));
    [HttpGet("segments"), RequirePermission("Client.Read")]
    public IActionResult Segments() => Ok(Envelope(EnterpriseDataService.RelationshipSegments(), "Segmentos calculados disponíveis."));
    [HttpGet("segments/{key}/clients"), RequirePermission("Client.Read")]
    public Task<IActionResult> SegmentClients(string key, CancellationToken ct) => Safe(async () => Ok(Envelope(await data.SegmentClientsAsync(key, ct), "Clientes do segmento carregados.")));
    [HttpGet("campaigns"), RequirePermission("Campaign.Read")]
    public Task<IActionResult> Campaigns(CancellationToken ct) => Safe(async () => Ok(Envelope(await data.ListAsync("campaigns", ct), "Campanhas internas carregadas.")));
    [HttpPost("campaigns"), RequirePermission("Campaign.Create")]
    public Task<IActionResult> CreateCampaign(JsonElement payload, CancellationToken ct) => Safe(async () => Ok(Envelope(await data.CreateAsync("campaigns", payload, ct), "Campanha interna criada; nenhum envio externo foi realizado.")));
}
