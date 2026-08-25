using System.Text.Json;
using BarberSync.Api.Services.Enterprise;
using BarberSync.Api.Services.Growth;
using BarberSync.Api.Security;
using BarberSync.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Authorize]
[Route("api/campaigns")]
public sealed class CampaignsController(EnterpriseDataService data, GrowthService growth, ICurrentUserContext currentUser, ILogger<CampaignsController> logger) : EnterpriseCrudController(data, logger, "campaigns")
{
    [HttpGet] public Task<IActionResult> GetAll(CancellationToken cancellationToken) => List(cancellationToken);
    [HttpGet("{id:guid}")] public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) => Get(id, cancellationToken);
    [HttpPost] public Task<IActionResult> CreateCampaign([FromBody] JsonElement payload, CancellationToken cancellationToken) => Create(payload, cancellationToken);
    [HttpPut("{id:guid}")] public Task<IActionResult> UpdateCampaign(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken) => Update(id, payload, cancellationToken);
    [HttpDelete("{id:guid}")] public Task<IActionResult> DeleteCampaign(Guid id, CancellationToken cancellationToken) => Delete(id, cancellationToken);
    [HttpPost("preview-audience"), Authorize(Roles = "Owner,Manager")]
    public async Task<IActionResult> PreviewAudience([FromBody] AudienceFilter filter, CancellationToken cancellationToken) =>
        Ok(await growth.PreviewAudienceAsync(currentUser.TenantId, currentUser.BranchId, filter, cancellationToken));
}
