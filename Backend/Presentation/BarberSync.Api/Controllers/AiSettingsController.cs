using BarberSync.Api.Services.Recognition;
using BarberSync.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Route("api/system/ai-settings"), Authorize(Roles="Owner,SuperAdmin,Admin")]
public sealed class AiSettingsController(IAiProvider provider, IServiceRecognitionService recognition, ICurrentUserContext user, ILogger<AiSettingsController> logger) : ControllerBase
{
    private static readonly object TestStateLock = new();
    private static readonly Dictionary<Guid, ProviderTestState> LastTestsByTenant = [];

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        ProviderTestState? lastTest;
        lock (TestStateLock) LastTestsByTenant.TryGetValue(user.TenantId, out lastTest);
        return Ok(new { provider=provider.Name, configured=provider.IsConfigured, model=(string?)null, lastTest, cameras=(await recognition.ListAsync(user.TenantId,user.BranchId,"camera_devices",ct)).Count, stations=(await recognition.ListAsync(user.TenantId,user.BranchId,"service_stations",ct)).Count, secretsExposed=false });
    }

    [HttpPost("test")]
    public async Task<IActionResult> Test(CancellationToken ct)
    {
        var testedAt = DateTimeOffset.UtcNow;
        if (!provider.IsConfigured)
        {
            SaveTest(user.TenantId, new(testedAt, false, "NotConfigured", HttpContext.TraceIdentifier));
            return UnprocessableEntity(new { connected=false, testedAt, status="NotConfigured", message="Provider não configurado. Nenhuma chamada externa foi realizada.", traceId=HttpContext.TraceIdentifier });
        }

        try
        {
            var connected = await provider.TestAsync(ct);
            SaveTest(user.TenantId, new(testedAt, connected, connected ? "Connected" : "Unavailable", connected ? null : HttpContext.TraceIdentifier));
            return connected
                ? Ok(new { connected, testedAt, status="Connected" })
                : StatusCode(StatusCodes.Status503ServiceUnavailable, new { connected, testedAt, status="Unavailable", message="Provider indisponível. Tente novamente mais tarde.", traceId=HttpContext.TraceIdentifier });
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "AI provider connection test failed. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            SaveTest(user.TenantId, new(testedAt, false, "Error", HttpContext.TraceIdentifier));
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { connected=false, testedAt, status="Error", message="Não foi possível testar o provider. Tente novamente mais tarde.", traceId=HttpContext.TraceIdentifier });
        }
    }

    private static void SaveTest(Guid tenantId, ProviderTestState state) { lock (TestStateLock) LastTestsByTenant[tenantId] = state; }
    private sealed record ProviderTestState(DateTimeOffset TestedAt, bool Connected, string Status, string? TraceId);
}
