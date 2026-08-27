using System.Text.Json;
using BarberSync.Api.Security;
using BarberSync.Api.Services.Clients360;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Authorize, Route("api/clients360")]
public sealed class Clients360Controller(Clients360Store store, ILogger<Clients360Controller> logger) : ControllerBase
{
    [HttpGet("dashboard"), RequirePermission("Clients360.Read")] public Task<IActionResult> Dashboard(CancellationToken ct) => Safe(() => store.Dashboard(ct));
    [HttpGet("search"), RequirePermission("Clients360.Read")] public Task<IActionResult> Search([FromQuery] string? q, CancellationToken ct) => Safe(() => store.Search(q, ct));
    [HttpGet("filter-options"), RequirePermission("Clients360.Read")] public Task<IActionResult> Options(CancellationToken ct) => Safe(() => store.Options(ct));
    [HttpGet("{clientId:guid}/profile"), RequirePermission("Clients360.Read")] public Task<IActionResult> Profile(Guid clientId, CancellationToken ct) => Safe(() => store.Profile(clientId, ct));
    [HttpGet("{clientId:guid}/timeline"), RequirePermission("Clients360.Sensitive.Read")] public Task<IActionResult> Timeline(Guid clientId, CancellationToken ct) => Safe(() => store.Timeline(clientId, ct));
    [HttpGet("{clientId:guid}/{collection:regex(technical-sheets|anamnesis|visual-records|consents|budgets|treatment-plans|follow-ups)}"), RequirePermission("Clients360.Read")] public Task<IActionResult> List(Guid clientId, string collection, CancellationToken ct) => Safe(() => store.List(clientId, collection, ct));
    [HttpPost("{clientId:guid}/{collection:regex(technical-sheets|anamnesis|consents|budgets|treatment-plans|follow-ups)}"), RequirePermission("Clients360.Manage")] public Task<IActionResult> Create(Guid clientId, string collection, [FromBody] JsonElement body, CancellationToken ct) => Safe(() => store.Create(clientId, collection, body, ct), true);
    [HttpPut("{clientId:guid}/{collection:regex(technical-sheets|anamnesis|budgets|treatment-plans)}/{id:guid}"), RequirePermission("Clients360.Manage")]
    public IActionResult Update(Guid clientId, string collection, Guid id) => Problem(statusCode: 501, title: "Atualização integral indisponível", detail: "Crie uma nova versão para preservar a auditoria.", extensions: Trace());
    [HttpPost("{clientId:guid}/visual-records/{id:guid}/archive"), RequirePermission("Clients360.VisualRecords.Manage")] public Task<IActionResult> ArchiveVisual(Guid clientId, Guid id, [FromBody] JsonElement body, CancellationToken ct) => Transition(clientId,"visual-records",id,"archive",body,ct);
    [HttpPost("{clientId:guid}/consents/{id:guid}/revoke"), RequirePermission("Clients360.Consents.Manage")] public Task<IActionResult> Revoke(Guid clientId, Guid id, [FromBody] JsonElement body, CancellationToken ct) => Transition(clientId,"consents",id,"revoke",body,ct);
    [HttpPost("{clientId:guid}/budgets/{id:guid}/{action:regex(approve|reject)}"), RequirePermission("Clients360.Budgets.Manage")] public Task<IActionResult> BudgetTransition(Guid clientId, Guid id, string action, [FromBody] JsonElement body, CancellationToken ct) => Transition(clientId,"budgets",id,action,body,ct);
    [HttpPost("{clientId:guid}/budgets/{id:guid}/convert-to-order"), RequirePermission("Clients360.Budgets.Manage")] public IActionResult ConvertBudget(Guid clientId, Guid id) => Problem(statusCode: 409, title: "Confirmação no PDV necessária", detail: "O snapshot deve ser confirmado na comanda; nenhuma cobrança automática ocorreu.", extensions: new Dictionary<string,object?>{{"traceId",HttpContext.TraceIdentifier},{"orderConfirmationRequired",true}});
    [HttpPost("{clientId:guid}/treatment-plans/{id:guid}/{action:regex(complete|cancel)}"), RequirePermission("Clients360.TreatmentPlans.Manage")] public Task<IActionResult> TreatmentTransition(Guid clientId, Guid id, string action, [FromBody] JsonElement body, CancellationToken ct) => Transition(clientId,"treatment-plans",id,action,body,ct);
    [HttpPost("{clientId:guid}/follow-ups/{id:guid}/complete"), RequirePermission("Clients360.Manage")] public Task<IActionResult> CompleteFollowUp(Guid clientId, Guid id, CancellationToken ct) => Transition(clientId,"follow-ups",id,"complete",JsonDocument.Parse("{}").RootElement,ct);

    private Task<IActionResult> Transition(Guid clientId,string collection,Guid id,string action,JsonElement body,CancellationToken ct) => Safe(() => store.Transition(clientId,collection,id,action,body,ct));
    private Dictionary<string,object?> Trace() => new(){{"traceId",HttpContext.TraceIdentifier}};
    private async Task<IActionResult> Safe(Func<Task<JsonElement>> operation,bool created=false)
    {
        try { var data=await operation(); return StatusCode(created?201:200,new { success=true,data,traceId=HttpContext.TraceIdentifier }); }
        catch(Clients360ValidationException ex){return Problem(statusCode:400,title:"Dados inválidos",detail:ex.Message,extensions:Trace());}
        catch(KeyNotFoundException ex){return Problem(statusCode:404,title:"Registro não encontrado",detail:ex.Message,extensions:Trace());}
        catch(Exception ex){logger.LogError(ex,"Falha no Cliente 360. TraceId {TraceId}",HttpContext.TraceIdentifier);return Problem(statusCode:500,title:"Falha ao processar Cliente 360",detail:"Tente novamente ou informe o traceId ao suporte.",extensions:Trace());}
    }
}
