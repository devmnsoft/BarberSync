using System.Text.Json;
using BarberSync.Api.Services.ClientPortal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BarberSync.Api.Controllers;

[ApiController,AllowAnonymous,Route("api/client-portal/auth")]
public sealed class ClientPortalAuthController(ClientPortalStore store):ControllerBase
{
 [HttpPost("request-code"),EnableRateLimiting("login")] public async Task<IActionResult> Request([FromBody] RequestCode x,CancellationToken ct){var result=await store.RequestCode(x.BranchCode,x.Destination,x.DestinationType,HttpContext,ct);return Ok(new{success=true,message=result.Message,data=new{deliveryStatus=result.DeliveryStatus,developmentCode=result.DevelopmentCode},traceId=HttpContext.TraceIdentifier});}
 [HttpPost("verify-code"),EnableRateLimiting("login")] public async Task<IActionResult> Verify([FromBody] VerifyCode x,CancellationToken ct){var token=await store.VerifyCode(x.BranchCode,x.Destination,x.Code,HttpContext,ct);return token is null?Problem(statusCode:401,title:"Código inválido ou expirado",detail:"Solicite um novo código e tente novamente.",extensions:new Dictionary<string,object?>{{"traceId",HttpContext.TraceIdentifier}}):Ok(new{success=true,data=new{accessToken=token,expiresIn=28800},traceId=HttpContext.TraceIdentifier});}
 [HttpPost("logout")] public async Task<IActionResult> Logout(CancellationToken ct){var s=await store.Authenticate(HttpContext,ct);if(s is not null)await store.Logout(s,ct);return NoContent();}
 public sealed record RequestCode(string BranchCode,string Destination,string DestinationType="Email");public sealed record VerifyCode(string BranchCode,string Destination,string Code);
}

[ApiController,AllowAnonymous,Route("api/client-portal")]
public sealed class ClientPortalController(ClientPortalStore store):ControllerBase
{
 [HttpGet("home")]public Task<IActionResult> Home(CancellationToken ct)=>Read(s=>store.Home(s,ct),ct);
 [HttpGet("profile")]public Task<IActionResult> Profile(CancellationToken ct)=>Read(async s=>(await store.Home(s,ct)).GetProperty("profile"),ct);
 [HttpGet("appointments")]public Task<IActionResult> Appointments(CancellationToken ct)=>List("appointments",ct);
 [HttpGet("history")]public Task<IActionResult> History(CancellationToken ct)=>List("history",ct);
 [HttpGet("consents")]public Task<IActionResult> Consents(CancellationToken ct)=>List("consents",ct);
 [HttpGet("anamnesis")]public Task<IActionResult> Anamnesis(CancellationToken ct)=>List("anamnesis",ct);
 [HttpGet("budgets")]public Task<IActionResult> Budgets(CancellationToken ct)=>List("budgets",ct);
 [HttpGet("treatment-plans")]public Task<IActionResult> Plans(CancellationToken ct)=>List("treatment-plans",ct);
 [HttpGet("payments")]public Task<IActionResult> Payments(CancellationToken ct)=>List("payments",ct);
 [HttpGet("benefits")]public Task<IActionResult> Benefits(CancellationToken ct)=>List("benefits",ct);
 [HttpGet("reviews")]public Task<IActionResult> Reviews(CancellationToken ct)=>List("reviews",ct);
 [HttpGet("preferences")]public Task<IActionResult> Preferences(CancellationToken ct)=>List("preferences",ct);
 [HttpGet("support")]public Task<IActionResult> Support(CancellationToken ct)=>List("support",ct);
 [HttpPost("appointments/{id:guid}/cancel")]public Task<IActionResult> Cancel(Guid id,[FromBody]JsonElement b,CancellationToken ct)=>Action(s=>store.Act(s,"cancel",id,b,HttpContext,ct),ct);
 [HttpPost("appointments/{id:guid}/reschedule")]public Task<IActionResult> Reschedule(Guid id,[FromBody]JsonElement b,CancellationToken ct)=>Action(s=>store.Act(s,"reschedule",id,b,HttpContext,ct),ct);
 [HttpPost("consents/{id:guid}/accept")]public Task<IActionResult> Accept(Guid id,[FromBody]JsonElement b,CancellationToken ct)=>Action(s=>store.Act(s,"consent-accept",id,b,HttpContext,ct),ct);
 [HttpPost("consents/{id:guid}/revoke")]public Task<IActionResult> Revoke(Guid id,[FromBody]JsonElement b,CancellationToken ct)=>Action(s=>store.Act(s,"consent-revoke",id,b,HttpContext,ct),ct);
 [HttpPost("budgets/{id:guid}/approve")]public Task<IActionResult> Approve(Guid id,[FromBody]JsonElement b,CancellationToken ct)=>Action(s=>store.Act(s,"budget-approve",id,b,HttpContext,ct),ct);
 [HttpPost("budgets/{id:guid}/reject")]public Task<IActionResult> Reject(Guid id,[FromBody]JsonElement b,CancellationToken ct)=>Action(s=>store.Act(s,"budget-reject",id,b,HttpContext,ct),ct);
 [HttpPost("payments/{id:guid}/confirm-intent")]public Task<IActionResult> Intent(Guid id,[FromBody]JsonElement b,CancellationToken ct)=>Action(s=>store.Act(s,"payment-intent",id,b,HttpContext,ct),ct);
 [HttpPost("reviews")]public Task<IActionResult> Review([FromBody]JsonElement b,CancellationToken ct)=>Action(s=>store.CreateReview(s,b,HttpContext,ct),ct);
 [HttpPut("preferences")]public Task<IActionResult> SavePreferences([FromBody]JsonElement b,CancellationToken ct)=>Action(s=>store.SavePreferences(s,b,HttpContext,ct),ct);
 [HttpPost("support")]public Task<IActionResult> OpenSupport([FromBody]JsonElement b,CancellationToken ct)=>Action(s=>store.OpenSupport(s,b,HttpContext,ct),ct);
 [HttpPost("support/{id:guid}/messages")]public Task<IActionResult> Message(Guid id,[FromBody]JsonElement b,CancellationToken ct)=>Action(s=>store.SupportMessage(s,id,b.TryGetProperty("message",out var m)?m.ToString():"",HttpContext,ct),ct);
 private Task<IActionResult> List(string kind,CancellationToken ct)=>Read(s=>store.List(s,kind,ct),ct);
 private async Task<IActionResult> Read(Func<PortalScope,Task<JsonElement>> action,CancellationToken ct){var s=await store.Authenticate(HttpContext,ct);if(s is null)return UnauthorizedProblem();try{return Ok(new{success=true,data=await action(s),traceId=HttpContext.TraceIdentifier});}catch(PortalValidationException e){return ValidationProblem(e.Message);}}
 private async Task<IActionResult> Action(Func<PortalScope,Task> action,CancellationToken ct){var s=await store.Authenticate(HttpContext,ct);if(s is null)return UnauthorizedProblem();try{await action(s);return Ok(new{success=true,message="Sua solicitação foi registrada.",traceId=HttpContext.TraceIdentifier});}catch(PortalValidationException e){return ValidationProblem(e.Message);}}
 private IActionResult UnauthorizedProblem()=>Problem(statusCode:401,title:"Acesso expirado",detail:"Entre novamente para continuar.",extensions:new Dictionary<string,object?>{{"traceId",HttpContext.TraceIdentifier}});
 private IActionResult ValidationProblem(string detail)=>Problem(statusCode:422,title:"Não foi possível concluir",detail:detail,extensions:new Dictionary<string,object?>{{"traceId",HttpContext.TraceIdentifier}});
}
