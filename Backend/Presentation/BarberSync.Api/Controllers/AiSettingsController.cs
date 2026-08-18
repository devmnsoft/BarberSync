using BarberSync.Api.Services.Recognition;
using BarberSync.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Route("api/system/ai-settings"), Authorize(Roles="Owner,SuperAdmin,Admin")]
public sealed class AiSettingsController(IAiProvider provider, IServiceRecognitionService recognition, ICurrentUserContext user) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> Get(CancellationToken ct) => Ok(new { provider=provider.Name, configured=provider.IsConfigured, model=(string?)null, lastCheckedAt=(DateTimeOffset?)null, cameras=(await recognition.ListAsync(user.TenantId,user.BranchId,"camera_devices",ct)).Count, stations=(await recognition.ListAsync(user.TenantId,user.BranchId,"service_stations",ct)).Count, secretsExposed=false });
    [HttpPost("test")] public async Task<IActionResult> Test(CancellationToken ct) => provider.IsConfigured ? Ok(new{connected=await provider.TestAsync(ct)}) : UnprocessableEntity(new{connected=false,message="Provider não configurado. Nenhuma chamada externa foi realizada.",traceId=HttpContext.TraceIdentifier});
}
