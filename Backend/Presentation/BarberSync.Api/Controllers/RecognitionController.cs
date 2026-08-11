using BarberSync.Api.Security;
using BarberSync.Api.Services.Recognition;
using BarberSync.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Authorize(Roles="Owner,Manager,Reception"), Route("api/service-recognition")]
public sealed class RecognitionController(IServiceRecognitionService service, ICurrentUserContext user) : ControllerBase
{
    public sealed record SuggestRequest(Guid? CameraDeviceId, Guid? AppointmentId, string[] Signals, bool HasCameraConsent);

    [HttpPost("suggestions")]
    public async Task<IActionResult> Suggest([FromBody] SuggestRequest request, CancellationToken ct)
    {
        if (!request.HasCameraConsent) return Problem("Consentimento/configuração de câmera é obrigatório.",statusCode:422);
        if (request.Signals.Length is 0) return BadRequest(new { message="Ao menos um sinal operacional é obrigatório." });
        var evt = new ServiceRecognitionEvent(Guid.NewGuid(),user.TenantId,user.BranchId,request.CameraDeviceId,request.AppointmentId,DateTimeOffset.UtcNow,request.Signals.ToHashSet(StringComparer.OrdinalIgnoreCase));
        var suggestion = await service.SuggestAsync(evt,ct);
        return suggestion is null ? Ok(new { eventId=evt.Id, suggestion=(object?)null, requiresHumanConfirmation=true }) : Ok(new { eventId=evt.Id, suggestion, requiresHumanConfirmation=true, automaticCharge=false });
    }

    [HttpPost("detect"), Consumes("multipart/form-data")]
    public IActionResult DetectImage() => StatusCode(StatusCodes.Status501NotImplemented,new { message="Provider de reconhecimento de imagem não configurado. Nenhuma imagem foi armazenada." });
}
