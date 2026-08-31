using System.Data.Common;
using System.Globalization;
using System.Text;
using BarberSync.Api.Security;
using BarberSync.Api.Services.Team;
using BarberSync.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Authorize, Route("api/command-center")]
public sealed class CommandCenterController(TeamDataService data) : ControllerBase
{
    [HttpGet("dashboard"), RequirePermission("CommandCenter.Read")]
    public async Task<IActionResult> Dashboard(CancellationToken ct) => Ok(new { success=true, data=await Summary(ct), traceId=HttpContext.TraceIdentifier });
    [HttpGet("executive"), RequirePermission("CommandCenter.Read")]
    public async Task<IActionResult> Executive(CancellationToken ct) => Ok(new { success=true, data=await Summary(ct), traceId=HttpContext.TraceIdentifier });
    [HttpGet("operations"), RequirePermission("CommandCenter.Read")]
    public async Task<IActionResult> Operations(CancellationToken ct) => Ok(new { success=true, data=await data.QueryAsync(@"select
      (select count(*) from barber.appointments where tenant_id=@tenant and branch_id=@branch and scheduled_start::date=current_date) appointments_today,
      (select count(*) from barber.appointments where tenant_id=@tenant and branch_id=@branch and status='NoShow' and scheduled_start::date=current_date) no_shows,
      (select count(*) from barber.service_orders where tenant_id=@tenant and branch_id=@branch and status='Open') open_orders,
      (select count(*) from barber.command_center_tasks where tenant_id=@tenant and branch_id=@branch and status in('Pending','InProgress') and due_at<now()) overdue_tasks,
      (select count(*) from barber.command_center_alerts where tenant_id=@tenant and branch_id=@branch and status in('Open','Acknowledged') and severity='Critical') critical_alerts",null,ct), sourceStatus="Available", traceId=HttpContext.TraceIdentifier });
    [HttpGet("filter-options"), RequirePermission("CommandCenter.Read")]
    public async Task<IActionResult> Options(CancellationToken ct) => Ok(new { success=true, data=new { users=await data.QueryAsync("select id,coalesce(full_name,email) name from barber.users where tenant_id=@tenant and branch_id=@branch order by name",null,ct), modules=new[]{"Agenda","Operation","Finance","Inventory","Team","Clients360","Portal","Club","Quality","Marketing","Partners","Communication","AI","Governance","Readiness"}, severities=new[]{"Low","Medium","High","Critical"} }, traceId=HttpContext.TraceIdentifier });
    private async Task<IReadOnlyList<Dictionary<string,object?>>> Summary(CancellationToken ct) => await data.QueryAsync(@"select s.*, 'Available' source_status from barber.command_center_snapshots s where s.tenant_id=@tenant and s.branch_id=@branch order by snapshot_date desc,created_at desc limit 1",null,ct);
}

[ApiController, Authorize, Route("api/command-center/alerts")]
public sealed class CommandCenterAlertsController(TeamDataService data, ICurrentUserContext user) : ControllerBase
{
    private static readonly HashSet<string> States=["Open","Acknowledged","Resolved","Dismissed"];
    [HttpGet, RequirePermission("CommandCenter.Read")] public async Task<IActionResult> List([FromQuery]string? status,CancellationToken ct){if(status is not null&&!States.Contains(status))return Invalid("status","Status inválido.");return Ok(new{success=true,data=await data.QueryAsync("select * from barber.command_center_alerts where tenant_id=@tenant and branch_id=@branch and (@status is null or status=@status) order by case severity when 'Critical' then 1 when 'High' then 2 when 'Medium' then 3 else 4 end,created_at desc",c=>Add(c,"status",status),ct),traceId=HttpContext.TraceIdentifier});}
    [HttpPost("{id:guid}/acknowledge"),RequirePermission("CommandCenter.Alerts.Manage")] public Task<IActionResult> Acknowledge(Guid id,CancellationToken ct)=>Transition(id,"Acknowledged",null,ct);
    [HttpPost("{id:guid}/resolve"),RequirePermission("CommandCenter.Alerts.Manage")] public Task<IActionResult> Resolve(Guid id,AlertActionRequest r,CancellationToken ct)=>Transition(id,"Resolved",r.Reason,ct);
    [HttpPost("{id:guid}/dismiss"),RequirePermission("CommandCenter.Alerts.Manage")] public Task<IActionResult> Dismiss(Guid id,AlertActionRequest r,CancellationToken ct)=>Transition(id,"Dismissed",r.Reason,ct);
    private async Task<IActionResult> Transition(Guid id,string target,string? reason,CancellationToken ct){var rows=await data.QueryAsync("select severity,status from barber.command_center_alerts where id=@id and tenant_id=@tenant and branch_id=@branch",c=>Add(c,"id",id),ct);if(rows.Count==0)return NotFoundProblem();var current=Convert.ToString(rows[0]["status"]);var severity=Convert.ToString(rows[0]["severity"]);if ((target == "Acknowledged" && current != "Open") || ((target is "Resolved" or "Dismissed") && (current is not ("Open" or "Acknowledged"))))return Invalid("status","A transição solicitada não é permitida.");if (target == "Dismissed" && (severity is "High" or "Critical") && string.IsNullOrWhiteSpace(reason))return Invalid("reason","Alertas High ou Critical exigem motivo para descarte.");await data.WriteAsync("update barber.command_center_alerts set status=@target,acknowledged_at=case when @target='Acknowledged' then now() else acknowledged_at end,resolved_at=case when @target='Resolved' then now() else resolved_at end,dismissed_at=case when @target='Dismissed' then now() else dismissed_at end,metadata_json=case when @reason is null then metadata_json else metadata_json||jsonb_build_object('actionReason',@reason,'actionBy',@user) end where id=@id and tenant_id=@tenant and branch_id=@branch","CommandCenterAlertTransitioned","command_center_alerts",id,null,c=>{Add(c,"target",target);Add(c,"reason",reason?.Trim());Add(c,"user",user.UserId);},ct);return Ok(new{success=true,traceId=HttpContext.TraceIdentifier});}
    private IActionResult NotFoundProblem()=>Problem(statusCode:404,title:"Alerta não encontrado",detail:"O alerta não existe nesta unidade.",extensions:new Dictionary<string,object?>{{"traceId",HttpContext.TraceIdentifier}});
    private IActionResult Invalid(string field,string message)=>BadRequest(new ProblemDetails{Title="Dados inválidos",Detail=message,Status=400,Extensions={{"traceId",HttpContext.TraceIdentifier},{"field",field}}});
    private static void Add(DbCommand c,string n,object? v)=>TeamDataService.Add(c,n,v);
}

[ApiController,Authorize,Route("api/command-center/incidents")]
public sealed class CommandCenterIncidentsController(TeamDataService data,ICurrentUserContext user):ControllerBase
{
 private static readonly HashSet<string> Severity=["Low","Medium","High","Critical"]; private static readonly HashSet<string> Status=["Open","Investigating","Resolved","Cancelled"];
 [HttpGet,RequirePermission("CommandCenter.Read")]public async Task<IActionResult> List(CancellationToken ct)=>Ok(new{success=true,data=await data.QueryAsync("select i.*,u.full_name assigned_name from barber.command_center_incidents i left join barber.users u on u.id=i.assigned_to where i.tenant_id=@tenant and i.branch_id=@branch order by case i.severity when 'Critical' then 1 when 'High' then 2 else 3 end,i.created_at desc",null,ct),traceId=HttpContext.TraceIdentifier});
 [HttpPost,RequirePermission("CommandCenter.Incidents.Manage")]public Task<IActionResult>Create(IncidentRequest r,CancellationToken ct)=>Save(Guid.NewGuid(),r,false,ct);
 [HttpPut("{id:guid}"),RequirePermission("CommandCenter.Incidents.Manage")]public Task<IActionResult>Update(Guid id,IncidentRequest r,CancellationToken ct)=>Save(id,r,true,ct);
 [HttpPost("{id:guid}/resolve"),RequirePermission("CommandCenter.Incidents.Manage")]public async Task<IActionResult>Resolve(Guid id,ResolveIncidentRequest r,CancellationToken ct){if(string.IsNullOrWhiteSpace(r.ResolutionNotes))return Invalid("resolutionNotes","Informe a resolução do incidente.");await data.WriteAsync("update barber.command_center_incidents set status='Resolved',resolution_notes=@notes,resolved_at=now(),updated_at=now() where id=@id and tenant_id=@tenant and branch_id=@branch","CommandCenterIncidentResolved","command_center_incidents",id,null,c=>Add(c,"notes",r.ResolutionNotes.Trim()),ct);return Ok(new{success=true,traceId=HttpContext.TraceIdentifier});}
 private async Task<IActionResult>Save(Guid id,IncidentRequest r,bool update,CancellationToken ct){if(!Severity.Contains(r.Severity)||!Status.Contains(r.Status)||string.IsNullOrWhiteSpace(r.Title)||string.IsNullOrWhiteSpace(r.Description)||r.Status=="Investigating"&&r.AssignedTo is null)return Invalid("incident","Informe título, descrição, severidade e responsável ao investigar.");var sql=update?"update barber.command_center_incidents set incident_type=@type,source_module=@module,severity=@severity,title=@title,description=@description,status=@status,assigned_to=@assigned,updated_at=now() where id=@id and tenant_id=@tenant and branch_id=@branch":"insert into barber.command_center_incidents(id,tenant_id,branch_id,incident_type,source_module,severity,title,description,status,assigned_to,created_by) values(@id,@tenant,@branch,@type,@module,@severity,@title,@description,@status,@assigned,@user)";await data.WriteAsync(sql,"CommandCenterIncidentSaved","command_center_incidents",id,null,c=>{Add(c,"type",r.IncidentType.Trim());Add(c,"module",r.SourceModule.Trim());Add(c,"severity",r.Severity);Add(c,"title",r.Title.Trim());Add(c,"description",r.Description.Trim());Add(c,"status",r.Status);Add(c,"assigned",r.AssignedTo);Add(c,"user",user.UserId);},ct);return Ok(new{success=true,data=new{id},traceId=HttpContext.TraceIdentifier});}
 private IActionResult Invalid(string f,string m)=>BadRequest(new ProblemDetails{Title="Dados inválidos",Detail=m,Status=400,Extensions={{"traceId",HttpContext.TraceIdentifier},{"field",f}}});private static void Add(DbCommand c,string n,object?v)=>TeamDataService.Add(c,n,v);
}

[ApiController,Authorize,Route("api/command-center/tasks")]
public sealed class CommandCenterTasksController(TeamDataService data,ICurrentUserContext user):ControllerBase
{
 private static readonly HashSet<string> Priority=["Low","Normal","High","Urgent"];private static readonly HashSet<string>Status=["Pending","InProgress","Done","Cancelled"];
 [HttpGet,RequirePermission("CommandCenter.Read")]public async Task<IActionResult>List(CancellationToken ct)=>Ok(new{success=true,data=await data.QueryAsync("select t.*,u.full_name assigned_name,(t.status in('Pending','InProgress') and t.due_at<now()) overdue from barber.command_center_tasks t left join barber.users u on u.id=t.assigned_to where t.tenant_id=@tenant and t.branch_id=@branch order by overdue desc,due_at nulls last,created_at desc",null,ct),traceId=HttpContext.TraceIdentifier});
 [HttpPost,RequirePermission("CommandCenter.Tasks.Manage")]public Task<IActionResult>Create(CommandTaskRequest r,CancellationToken ct)=>Save(Guid.NewGuid(),r,false,ct);[HttpPut("{id:guid}"),RequirePermission("CommandCenter.Tasks.Manage")]public Task<IActionResult>Update(Guid id,CommandTaskRequest r,CancellationToken ct)=>Save(id,r,true,ct);
 [HttpPost("{id:guid}/complete"),RequirePermission("CommandCenter.Tasks.Manage")]public async Task<IActionResult>Complete(Guid id,CancellationToken ct){await data.WriteAsync("update barber.command_center_tasks set status='Done',completed_at=now(),completed_by=@user,updated_at=now() where id=@id and tenant_id=@tenant and branch_id=@branch and status in('Pending','InProgress')","CommandCenterTaskCompleted","command_center_tasks",id,null,c=>Add(c,"user",user.UserId),ct);return Ok(new{success=true,traceId=HttpContext.TraceIdentifier});}
 private async Task<IActionResult>Save(Guid id,CommandTaskRequest r,bool update,CancellationToken ct){if(!Priority.Contains(r.Priority)||!Status.Contains(r.Status)||string.IsNullOrWhiteSpace(r.Title)||string.IsNullOrWhiteSpace(r.TaskType)||string.IsNullOrWhiteSpace(r.SourceModule))return BadRequest(new ProblemDetails{Title="Dados inválidos",Detail="Informe tipo, módulo, título, prioridade e status válidos.",Status=400,Extensions={{"traceId",HttpContext.TraceIdentifier}}});var sql=update?"update barber.command_center_tasks set task_type=@type,source_module=@module,title=@title,description=@description,priority=@priority,status=@status,assigned_to=@assigned,due_at=@due,updated_at=now() where id=@id and tenant_id=@tenant and branch_id=@branch":"insert into barber.command_center_tasks(id,tenant_id,branch_id,task_type,source_module,title,description,priority,status,assigned_to,due_at,created_by) values(@id,@tenant,@branch,@type,@module,@title,@description,@priority,@status,@assigned,@due,@user)";await data.WriteAsync(sql,"CommandCenterTaskSaved","command_center_tasks",id,null,c=>{Add(c,"type",r.TaskType.Trim());Add(c,"module",r.SourceModule.Trim());Add(c,"title",r.Title.Trim());Add(c,"description",r.Description?.Trim());Add(c,"priority",r.Priority);Add(c,"status",r.Status);Add(c,"assigned",r.AssignedTo);Add(c,"due",r.DueAt);Add(c,"user",user.UserId);},ct);return Ok(new{success=true,data=new{id},traceId=HttpContext.TraceIdentifier});}private static void Add(DbCommand c,string n,object?v)=>TeamDataService.Add(c,n,v);
}

[ApiController,Authorize,Route("api/command-center")]
public sealed class CommandCenterHealthController(TeamDataService data):ControllerBase
{[HttpGet("health"),RequirePermission("CommandCenter.Read")]public async Task<IActionResult>Health(CancellationToken ct)=>Ok(new{success=true,data=await data.QueryAsync("select integration_key,source_module,status,message,last_checked_at from barber.command_center_integration_checks where tenant_id=@tenant and branch_id=@branch order by source_module,integration_key",null,ct),sourceStatus="PersistedChecks",traceId=HttpContext.TraceIdentifier});[HttpGet("integrations"),RequirePermission("CommandCenter.Read")]public Task<IActionResult>Integrations(CancellationToken ct)=>Health(ct);}

[ApiController,Authorize,Route("api/command-center/reports")]
public sealed class CommandCenterReportsController(TeamDataService data):ControllerBase
{[HttpGet("export"),RequirePermission("CommandCenter.Reports.Export")]public async Task<IActionResult>Export(CancellationToken ct){var rows=await data.QueryAsync("select snapshot_date,revenue_amount,appointments_count,open_orders_count,pending_payments_count,low_stock_count,nps_score,critical_alerts_count from barber.command_center_snapshots where tenant_id=@tenant and branch_id=@branch order by snapshot_date desc",null,ct);var csv=new StringBuilder("data;receita;agendamentos;comandas_abertas;pagamentos_pendentes;estoque_critico;nps;alertas_criticos\r\n");foreach(var row in rows)csv.AppendLine(string.Join(';',row.Values.Select(v=>$"\"{Convert.ToString(v,CultureInfo.InvariantCulture)?.Replace("\"","\"\"")}\"")));return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(),"text/csv",$"central-controle-{DateTime.UtcNow:yyyyMMdd}.csv");}}

public sealed record AlertActionRequest(string? Reason);
public sealed record IncidentRequest(string IncidentType,string SourceModule,string Severity,string Title,string Description,string Status,Guid? AssignedTo);
public sealed record ResolveIncidentRequest(string ResolutionNotes);
public sealed record CommandTaskRequest(string TaskType,string SourceModule,string Title,string? Description,string Priority,string Status,Guid? AssignedTo,DateTimeOffset? DueAt);
