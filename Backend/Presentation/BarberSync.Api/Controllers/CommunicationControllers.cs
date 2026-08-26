using BarberSync.Api.Services.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController,Authorize,Route("api/communication")]
public sealed class CommunicationController(CommunicationService service):ControllerBase
{
 [HttpGet("dashboard")] public async Task<IActionResult> Dashboard(CancellationToken ct)=>Ok((await service.Dashboard(ct)).RootElement);
 [HttpGet("channels")] public async Task<IActionResult> Channels(CancellationToken ct)=>Ok((await service.List("communication_channels",ct)).RootElement);
}
[ApiController,Authorize,Route("api/communication/templates")]
public sealed class CommunicationTemplatesController(CommunicationService service):ControllerBase
{
 [HttpGet] public async Task<IActionResult> List(CancellationToken ct)=>Ok((await service.List("communication_templates",ct)).RootElement);
 [HttpPost] public async Task<IActionResult> Create(TemplateRequest request,CancellationToken ct)=>await Execute(async()=>Ok(new{id=await service.CreateTemplate(request,ct)}));
 [HttpPost("{id:guid}/deactivate")] public IActionResult Deactivate(Guid id)=>StatusCode(501,new{message="Use a edição de status; operação não concluída.",traceId=HttpContext.TraceIdentifier});
 private async Task<IActionResult> Execute(Func<Task<IActionResult>> action){try{return await action();}catch(CommunicationValidationException e){return BadRequest(new{message=e.Message,traceId=HttpContext.TraceIdentifier,errors=new{form=new[]{e.Message}}});}}
}
[ApiController,Authorize,Route("api/communication/campaigns")]
public sealed class CommunicationCampaignsController(CommunicationService service):ControllerBase
{
 [HttpGet] public async Task<IActionResult> List(CancellationToken ct)=>Ok((await service.List("communication_campaigns",ct)).RootElement);
 [HttpPost] public async Task<IActionResult> Create(CampaignRequest request,CancellationToken ct){try{return Ok(new{id=await service.CreateCampaign(request,ct)});}catch(CommunicationValidationException e){return BadRequest(new{message=e.Message,traceId=HttpContext.TraceIdentifier,errors=new{form=new[]{e.Message}}});}}
}
[ApiController,Authorize,Route("api/communication/automations")]
public sealed class CommunicationAutomationsController(CommunicationService service):ControllerBase
{
 [HttpGet] public async Task<IActionResult> List(CancellationToken ct)=>Ok((await service.List("communication_automations",ct)).RootElement);
 [HttpPost] public async Task<IActionResult> Create(AutomationRequest request,CancellationToken ct){try{return Ok(new{id=await service.CreateAutomation(request,ct)});}catch(CommunicationValidationException e){return BadRequest(new{message=e.Message,traceId=HttpContext.TraceIdentifier,errors=new{form=new[]{e.Message}}});}}
}
[ApiController,Authorize,Route("api/communication/outbox")]
public sealed class CommunicationOutboxController(CommunicationService service):ControllerBase
{
 [HttpGet] public async Task<IActionResult> List(CancellationToken ct)=>Ok((await service.List("communication_outbox",ct)).RootElement);
 [HttpPost("{id:guid}/retry")] public async Task<IActionResult> Retry(Guid id,CancellationToken ct)=>await Change(id,"retry",ct);
 [HttpPost("{id:guid}/cancel")] public async Task<IActionResult> Cancel(Guid id,CancellationToken ct)=>await Change(id,"cancel",ct);
 private async Task<IActionResult> Change(Guid id,string action,CancellationToken ct)=>await service.ChangeOutbox(id,action,ct)>0?Ok(new{id}):Conflict(new{message="O status atual não permite esta ação.",traceId=HttpContext.TraceIdentifier});
}
[ApiController,Authorize,Route("api/notifications/inbox")]
public sealed class NotificationInboxController(CommunicationService service):ControllerBase
{
 [HttpGet] public async Task<IActionResult> List(CancellationToken ct)=>Ok((await service.List("notification_inbox",ct)).RootElement);
 [HttpPost("{id:guid}/read")] public async Task<IActionResult> Read(Guid id,CancellationToken ct)=>await service.Read(id,ct)>0?Ok(new{id}):NotFound(new{message="Notificação não encontrada.",traceId=HttpContext.TraceIdentifier});
 [HttpPost("read-all")] public async Task<IActionResult> ReadAll(CancellationToken ct)=>Ok(new{updated=await service.Read(null,ct)});
}
[ApiController,Authorize,Route("api/notifications/preferences")]
public sealed class NotificationPreferencesController(CommunicationService service):ControllerBase
{
 [HttpGet] public async Task<IActionResult> List(CancellationToken ct)=>Ok((await service.List("notification_preferences",ct)).RootElement);
 [HttpPut] public async Task<IActionResult> Put(PreferenceRequest request,CancellationToken ct){try{await service.ReplacePreferences(request,ct);return Ok(new{message="Preferências atualizadas."});}catch(CommunicationValidationException e){return BadRequest(new{message=e.Message,traceId=HttpContext.TraceIdentifier,errors=new{items=new[]{e.Message}}});}}
}
[ApiController,Authorize,Route("api/communication/reports")]
public sealed class CommunicationReportsController(CommunicationService service):ControllerBase
{
 [HttpGet("export")] public async Task<IActionResult> Export([FromQuery]string type,[FromQuery]DateOnly from,[FromQuery]DateOnly to,CancellationToken ct){if(type!="delivery")return BadRequest(new{message="Tipo de relatório inválido.",traceId=HttpContext.TraceIdentifier});try{return File(await service.Export(from,to,ct),"text/csv","communication-delivery.csv");}catch(CommunicationValidationException e){return BadRequest(new{message=e.Message,traceId=HttpContext.TraceIdentifier});}}
}
