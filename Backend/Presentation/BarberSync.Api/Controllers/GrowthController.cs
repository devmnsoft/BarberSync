using BarberSync.Api.Security;
using BarberSync.Api.Services.Growth;
using BarberSync.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Authorize, Route("api/growth")]
public sealed class GrowthController(GrowthService growth, ICurrentUserContext currentUser) : ControllerBase
{
    [HttpGet("clients/{clientId:guid}/360")]
    public async Task<IActionResult> Client360(Guid clientId, CancellationToken ct) => Ok(await growth.Client360Async(currentUser.TenantId, clientId, ct));

    [HttpGet("reactivation"), Authorize(Roles="Owner,Manager")]
    public async Task<IActionResult> Reactivation([FromQuery] int days=30, CancellationToken ct=default) => Ok(await growth.ReactivationAsync(currentUser.TenantId, currentUser.BranchId, days, ct));

    [HttpPost("campaigns/preview-audience"), Authorize(Roles="Owner,Manager")]
    public async Task<IActionResult> Preview([FromBody] AudienceFilter filter, CancellationToken ct) => Ok(await growth.PreviewAudienceAsync(currentUser.TenantId, currentUser.BranchId, filter, ct));

    [HttpGet("assistant/dashboard")]
    public async Task<IActionResult> DashboardInsights(CancellationToken ct) => Ok(await growth.GetDashboardAsync(currentUser.TenantId, currentUser.BranchId, ct));

    [HttpGet("assistant/clients/{clientId:guid}")]
    public async Task<IActionResult> ClientInsights(Guid clientId, CancellationToken ct) => Ok(await growth.GetClientAsync(currentUser.TenantId, clientId, ct));
}
