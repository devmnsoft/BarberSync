using System.Text.Json;
using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BarberSync.Api.Security;

namespace BarberSync.Api.Controllers;

[ApiController, Authorize]
[Route("api/clients")]
public sealed class ClientsController(EnterpriseDataService data, ILogger<ClientsController> logger) : EnterpriseCrudController(data, logger, "clients")
{
    [HttpGet] public Task<IActionResult> GetAll(CancellationToken cancellationToken) => List(cancellationToken);
    [HttpGet("{id:guid}")] public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) => Get(id, cancellationToken);
    [HttpGet("{id:guid}/commercial-benefits")]
    public async Task<IActionResult> GetCommercialBenefits(Guid id, CancellationToken cancellationToken) => Ok(await data.ClientCommercialBenefitsAsync(id, cancellationToken));
    [HttpPost] public Task<IActionResult> CreateClient([FromBody] JsonElement payload, CancellationToken cancellationToken) => Create(payload, cancellationToken);
    [HttpPut("{id:guid}")] public Task<IActionResult> UpdateClient(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken) => Update(id, payload, cancellationToken);
    [HttpDelete("{id:guid}")] public Task<IActionResult> DeleteClient(Guid id, CancellationToken cancellationToken) => Delete(id, cancellationToken);
    [HttpGet("{id:guid}/profile"), Authorize, RequirePermission("Client.Read")]
    public Task<IActionResult> Profile(Guid id, CancellationToken ct) => Safe(async () => (await data.ClientRelationshipAsync(id, ct)) is { } value ? Ok(Envelope(value, "Perfil completo carregado.")) : NotFound(Envelope(null, "Cliente não encontrado.", false)));
    [HttpPut("{id:guid}/profile"), Authorize, RequirePermission("Client.Update")]
    public Task<IActionResult> UpdateProfile(Guid id, [FromBody] JsonElement payload, CancellationToken ct) => Safe(async () => Ok(Envelope(await data.UpsertClientProfileAsync(id, payload, ct), "Perfil atualizado.")));
    [HttpGet("{id:guid}/timeline"), Authorize, RequirePermission("Client.Read")]
    public Task<IActionResult> Timeline(Guid id, CancellationToken ct) => Safe(async () => Ok(Envelope(await data.ClientTimelineAsync(id, ct), "Linha do tempo carregada.")));
}
