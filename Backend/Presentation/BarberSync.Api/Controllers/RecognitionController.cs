using System.Text.Json;
using BarberSync.Api.Services.Recognition;
using BarberSync.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Authorize(Roles="Owner,SuperAdmin,Admin,Manager,Reception"), Route("api/service-recognition")]
public sealed class ServiceRecognitionController(IServiceRecognitionService service, ICurrentUserContext user) : ControllerBase
{
    public sealed record EventRequest(Guid CameraDeviceId, Guid? StationId, Guid? AppointmentId, Guid? ProfessionalId, string[] Signals, bool HasCameraConsent);

    [HttpGet("suggestions")] public Task<IReadOnlyList<Dictionary<string,object?>>> Suggestions(CancellationToken ct) => Query("service_recognition_suggestions", "s", "JOIN barber.service_recognition_events e ON e.id=s.event_id", ct);
    [HttpGet("events")] public Task<IReadOnlyList<Dictionary<string,object?>>> Events(CancellationToken ct) => Query("service_recognition_events", "s", "", ct);
    [HttpGet("{resource:regex(^(cameras|stations|rules)$)}")] public Task<IReadOnlyList<Dictionary<string,object?>>> Resources(string resource,CancellationToken ct)=>service.ListAsync(user.TenantId,user.BranchId,Map(resource),ct);
    [HttpPost("{resource:regex(^(cameras|stations|rules)$)}")] public async Task<IActionResult> Create(string resource,[FromBody]JsonElement payload,CancellationToken ct)=>Ok(await service.CreateAsync(user.TenantId,user.BranchId,Map(resource),payload,ct));
    [HttpPost("events")]
    public async Task<IActionResult> Record([FromBody]EventRequest request,CancellationToken ct)
    { if(!request.HasCameraConsent)return UnprocessableEntity(new{message="Consentimento explícito é obrigatório. Imagens e biometria não são armazenadas.",traceId=HttpContext.TraceIdentifier});if(request.Signals.Length==0)return BadRequest(new{message="Informe sinais operacionais.",traceId=HttpContext.TraceIdentifier});var result=await service.RecordEventAsync(new(Guid.NewGuid(),user.TenantId,user.BranchId,request.CameraDeviceId,request.StationId,request.AppointmentId,request.ProfessionalId,DateTimeOffset.UtcNow,request.Signals),ct);return Ok(new{suggestion=result,requiresHumanConfirmation=true,automaticCharge=false});}
    [HttpPost("suggestions/{id:guid}/confirm")] public async Task<IActionResult> Confirm(Guid id,[FromBody]RecognitionDecision request,CancellationToken ct)=>Ok(new{serviceOrderId=await service.DecideAsync(user.TenantId,user.BranchId,user.UserId,request with{SuggestionId=id},true,HttpContext.TraceIdentifier,ct),automaticCharge=false});
    [HttpPost("suggestions/{id:guid}/reject")] public async Task<IActionResult> Reject(Guid id,[FromBody]RecognitionDecision request,CancellationToken ct){await service.DecideAsync(user.TenantId,user.BranchId,user.UserId,request with{SuggestionId=id},false,HttpContext.TraceIdentifier,ct);return Ok(new{rejected=true});}
    [HttpPost("detect")] public IActionResult Detect()=>UnprocessableEntity(new{message="Captura de imagem desabilitada nesta fase LGPD.",traceId=HttpContext.TraceIdentifier});
    private static string Map(string value)=>value switch{"cameras"=>"camera_devices","stations"=>"service_stations",_=>"recognition_rules"};
    private Task<IReadOnlyList<Dictionary<string,object?>>> Query(string table,string alias,string join,CancellationToken ct)=>service.ListAsync(user.TenantId,user.BranchId,table,ct);
}
