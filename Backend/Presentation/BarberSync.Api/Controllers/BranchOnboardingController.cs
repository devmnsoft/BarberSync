using System.Text.Json;
using BarberSync.Api.Security;
using BarberSync.Api.Services.Onboarding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Authorize, Route("api/branch-onboarding"), RequirePermission("Settings.Write")]
public sealed class BranchOnboardingController(BranchOnboardingService service) : ControllerBase
{
    [HttpGet] public async Task<ActionResult<OnboardingProgressDto>> Get(CancellationToken ct) => Ok(await service.GetAsync(ct));
    [HttpPut("steps/{step:int}")]
    public async Task<ActionResult<OnboardingProgressDto>> Save(int step, [FromBody] JsonElement payload, CancellationToken ct)
    {
        try { return Ok(await service.SaveStepAsync(step, payload, ct)); }
        catch (ArgumentException exception) { return ValidationProblem(title: "Revise os campos da etapa.", detail: exception.Message, statusCode: 422); }
    }
    [HttpPost("complete")]
    public async Task<ActionResult<OnboardingProgressDto>> Complete(CancellationToken ct)
    {
        try { return Ok(await service.CompleteAsync(ct)); }
        catch (InvalidOperationException exception) { return Conflict(new { success = false, message = exception.Message }); }
    }
}
