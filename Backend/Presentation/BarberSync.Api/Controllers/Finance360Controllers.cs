using BarberSync.Api.Security;
using BarberSync.Api.Services.Finance360;
using BarberSync.Api.Services.Team;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController,Authorize]
public abstract class Finance360ControllerBase : ControllerBase { protected object OkData(object? data)=>new{success=true,data}; }

[Route("api/finance360")]
public sealed class Finance360Controller(TeamDataService data):Finance360ControllerBase
{
 [HttpGet("dashboard"),RequirePermission("Finance360.Read")] public async Task<IActionResult> Dashboard(CancellationToken ct)=>Ok(OkData((await data.QueryAsync(@"select coalesce(sum(amount) filter(where direction='Credit' and status='Confirmed'),0) revenue,coalesce(sum(amount) filter(where direction='Debit' and status='Confirmed'),0) outflow,(select coalesce(sum(amount-paid_amount),0) from barber.finance_receivables where tenant_id=@tenant and branch_id=@branch and status in ('Open','PartiallyPaid','Overdue')) receivables,(select coalesce(sum(amount-paid_amount),0) from barber.finance_payables where tenant_id=@tenant and branch_id=@branch and status in ('Open','Scheduled','PartiallyPaid','Overdue')) payables,(select count(*) from barber.finance_delinquency_cases where tenant_id=@tenant and branch_id=@branch and status in ('Open','Negotiating')) delinquency from barber.finance_postings where tenant_id=@tenant and branch_id=@branch",null,ct)).Single()));
 [HttpGet("filter-options"),RequirePermission("Finance360.Read")] public async Task<IActionResult> Filters(CancellationToken ct)=>Ok(OkData(new{accounts=await data.QueryAsync("select id,name,account_type from barber.finance_accounts where tenant_id=@tenant and branch_id=@branch and status='Active' and deleted_at is null order by name",null,ct),categories=await data.QueryAsync("select id,name,category_type from barber.finance_categories where tenant_id=@tenant and branch_id=@branch and status='Active' and deleted_at is null order by name",null,ct),costCenters=await data.QueryAsync("select id,name from barber.finance_cost_centers where tenant_id=@tenant and branch_id=@branch and status='Active' and deleted_at is null order by name",null,ct)}));
}

[Route("api/finance360/receivables")]
public sealed class FinanceReceivablesController(AccountsReceivableService service,TeamDataService data):Finance360ControllerBase
{
 [HttpGet,RequirePermission("Finance360.Read")] public async Task<IActionResult> List([FromQuery]string? status,CancellationToken ct)=>Ok(OkData(await data.QueryAsync("select * from barber.finance_receivables where tenant_id=@tenant and branch_id=@branch and (@status is null or status=@status) order by due_at",c=>TeamDataService.Add(c,"status",status),ct)));
 [HttpPost,RequirePermission("Finance360.Receivables.Manage")] public async Task<IActionResult> Create(CreateReceivableRequest r,CancellationToken ct)=>Ok(OkData(await service.CreateReceivableAsync(r,ct)));
 [HttpPost("{id:guid}/mark-paid"),RequirePermission("Finance360.Receivables.Manage")] public async Task<IActionResult> Paid(Guid id,MarkReceivablePaidRequest r,CancellationToken ct)=>Ok(OkData(await service.MarkReceivablePaidAsync(r with{Id=id},ct)));
 [HttpPost("{id:guid}/mark-partially-paid"),RequirePermission("Finance360.Receivables.Manage")] public async Task<IActionResult> Partial(Guid id,MarkReceivablePartiallyPaidRequest r,CancellationToken ct)=>Ok(OkData(await service.MarkReceivablePartiallyPaidAsync(r with{Id=id},ct)));
 [HttpPost("{id:guid}/cancel"),RequirePermission("Finance360.Receivables.Manage")] public async Task<IActionResult> Cancel(Guid id,CancelReceivableRequest r,CancellationToken ct)=>Ok(OkData(await service.CancelReceivableAsync(r with{Id=id},ct)));
 [HttpGet("aging"),RequirePermission("Finance360.Read")] public async Task<IActionResult> Aging([FromQuery]DateOnly from,[FromQuery]DateOnly to,CancellationToken ct)=>Ok(OkData(await service.GetReceivableAgingAsync(new(from,to),ct)));
}

[Route("api/finance360/payables")]
public sealed class FinancePayablesController(AccountsPayableService service,TeamDataService data):Finance360ControllerBase
{
 [HttpGet,RequirePermission("Finance360.Read")] public async Task<IActionResult> List([FromQuery]string? status,CancellationToken ct)=>Ok(OkData(await data.QueryAsync("select * from barber.finance_payables where tenant_id=@tenant and branch_id=@branch and (@status is null or status=@status) order by due_at",c=>TeamDataService.Add(c,"status",status),ct)));
 [HttpPost,RequirePermission("Finance360.Payables.Manage")] public async Task<IActionResult> Create(CreatePayableRequest r,CancellationToken ct)=>Ok(OkData(await service.CreatePayableAsync(r,ct)));
 [HttpPost("{id:guid}/schedule"),RequirePermission("Finance360.Payables.Manage")] public async Task<IActionResult> Schedule(Guid id,SchedulePayableRequest r,CancellationToken ct)=>Ok(OkData(await service.SchedulePayableAsync(r with{Id=id},ct)));
 [HttpPost("{id:guid}/mark-paid"),RequirePermission("Finance360.Payables.Manage")] public async Task<IActionResult> Paid(Guid id,MarkPayablePaidRequest r,CancellationToken ct)=>Ok(OkData(await service.MarkPayablePaidAsync(r with{Id=id},ct)));
 [HttpPost("{id:guid}/mark-partially-paid"),RequirePermission("Finance360.Payables.Manage")] public async Task<IActionResult> Partial(Guid id,MarkPayablePartiallyPaidRequest r,CancellationToken ct)=>Ok(OkData(await service.MarkPayablePartiallyPaidAsync(r with{Id=id},ct)));
 [HttpPost("{id:guid}/cancel"),RequirePermission("Finance360.Payables.Manage")] public async Task<IActionResult> Cancel(Guid id,CancelPayableRequest r,CancellationToken ct)=>Ok(OkData(await service.CancelPayableAsync(r with{Id=id},ct)));
 [HttpGet("aging"),RequirePermission("Finance360.Read")] public async Task<IActionResult> Aging([FromQuery]DateOnly from,[FromQuery]DateOnly to,CancellationToken ct)=>Ok(OkData(await service.GetPayableAgingAsync(new(from,to),ct)));
}

[Route("api/finance360/reconciliation")]
public sealed class FinanceReconciliationController(ReconciliationService service,TeamDataService data):Finance360ControllerBase
{
 [HttpGet,RequirePermission("Finance360.Read")] public async Task<IActionResult> List(CancellationToken ct)=>Ok(OkData(await data.QueryAsync("select * from barber.finance_reconciliations where tenant_id=@tenant and branch_id=@branch order by created_at desc",null,ct)));
 [HttpPost("preview"),RequirePermission("Finance360.Reconciliation.Manage")] public async Task<IActionResult> Preview(ReconciliationPreviewRequest r,CancellationToken ct)=>Ok(OkData(await service.PreviewReconciliationAsync(r,ct)));
 [HttpPost("reconcile"),RequirePermission("Finance360.Reconciliation.Manage")] public async Task<IActionResult> Reconcile(ReconcilePaymentRequest r,CancellationToken ct)=>Ok(OkData(await service.ReconcilePaymentAsync(r,ct)));
 [HttpPost("mark-divergent"),RequirePermission("Finance360.Reconciliation.Manage")] public async Task<IActionResult> Divergent(MarkReconciliationDivergentRequest r,CancellationToken ct)=>Ok(OkData(await service.MarkAsDivergentAsync(r,ct)));
 [HttpPost("reverse"),RequirePermission("Finance360.Reconciliation.Manage")] public async Task<IActionResult> Reverse(ReverseReconciliationRequest r,CancellationToken ct)=>Ok(OkData(await service.ReverseReconciliationAsync(r,ct)));
}

[Route("api/finance360/cash-flow")]
public sealed class FinanceCashFlowController(CashFlowService service):Finance360ControllerBase
{
 [HttpGet,RequirePermission("Finance360.CashFlow.Read")] public async Task<IActionResult> Compare([FromQuery]DateOnly from,[FromQuery]DateOnly to,CancellationToken ct)=>Ok(OkData(await service.GetProjectedVsRealizedAsync(new(from,to),ct)));
 [HttpGet("projection"),RequirePermission("Finance360.CashFlow.Read")] public async Task<IActionResult> Projection([FromQuery]DateOnly from,[FromQuery]DateOnly to,CancellationToken ct)=>Ok(OkData(await service.GetProjectionAsync(new(from,to),ct)));
 [HttpGet("realized"),RequirePermission("Finance360.CashFlow.Read")] public async Task<IActionResult> Realized([FromQuery]DateOnly from,[FromQuery]DateOnly to,CancellationToken ct)=>Ok(OkData(await service.GetRealizedAsync(new(from,to),ct)));
 [HttpPost("snapshot"),RequirePermission("Finance360.Manage")] public async Task<IActionResult> Snapshot(CreateCashFlowSnapshotRequest r,CancellationToken ct)=>Ok(OkData(await service.CreateSnapshotAsync(r,ct)));
}

[Route("api/finance360/dre")]
public sealed class FinanceDreController(DreService service):Finance360ControllerBase
{
 [HttpGet,RequirePermission("Finance360.Dre.Read")] public async Task<IActionResult> Get([FromQuery]DateOnly from,[FromQuery]DateOnly to,CancellationToken ct)=>Ok(OkData(await service.GetDreAsync(new(from,to),ct)));
 [HttpPost("snapshot"),RequirePermission("Finance360.Manage")] public async Task<IActionResult> Snapshot(CreateDreSnapshotRequest r,CancellationToken ct)=>Ok(OkData(await service.CreateDreSnapshotAsync(r,ct)));
 [HttpGet("export"),RequirePermission("Finance360.Reports.Export")] public async Task<IActionResult> Export([FromQuery]DateOnly from,[FromQuery]DateOnly to,CancellationToken ct){var x=await service.ExportDreAsync(new(from,to),ct);return File(x.Content,"text/csv",x.FileName);}
}

[Route("api/finance360/audit")]
public sealed class FinanceAuditController(FinanceAuditService service):Finance360ControllerBase
{[HttpGet,RequirePermission("Finance360.Read")]public async Task<IActionResult> Search([FromQuery]DateOnly from,[FromQuery]DateOnly to,[FromQuery]string? eventType,CancellationToken ct)=>Ok(OkData(await service.SearchAsync(new(from,to,eventType),ct)));}

[Route("api/finance360/commissions")] public sealed class FinanceCommissionsController(TeamDataService d):Finance360ControllerBase {[HttpGet,RequirePermission("Finance360.Commissions.Read")]public async Task<IActionResult> Get(CancellationToken ct)=>Ok(OkData(await d.QueryAsync("select * from barber.commissions where tenant_id=@tenant and branch_id=@branch order by created_at desc",null,ct)));}
[Route("api/finance360/payroll")] public sealed class FinancePayrollController(TeamDataService d):Finance360ControllerBase {[HttpGet,RequirePermission("Finance360.Read")]public async Task<IActionResult> Get(CancellationToken ct)=>Ok(OkData(await d.QueryAsync("select * from barber.finance_payables where tenant_id=@tenant and branch_id=@branch and source_type='PayrollSettlement' order by due_at desc",null,ct)));}
[Route("api/finance360/partner-payouts")] public sealed class FinancePartnerPayoutsController(TeamDataService d):Finance360ControllerBase {[HttpGet,RequirePermission("Finance360.Read")]public async Task<IActionResult> Get(CancellationToken ct)=>Ok(OkData(await d.QueryAsync("select * from barber.finance_payables where tenant_id=@tenant and branch_id=@branch and source_type='PartnerPayout' order by due_at desc",null,ct)));}
[Route("api/finance360/delinquency")] public sealed class FinanceDelinquencyController(TeamDataService d):Finance360ControllerBase {[HttpGet,RequirePermission("Finance360.Read")]public async Task<IActionResult> Get(CancellationToken ct)=>Ok(OkData(await d.QueryAsync("select * from barber.finance_delinquency_cases where tenant_id=@tenant and branch_id=@branch order by due_at",null,ct)));[HttpPost("{id:guid}/assign"),RequirePermission("Finance360.Delinquency.Manage")]public async Task<IActionResult> Assign(Guid id,AssignRequest r,CancellationToken ct){await d.WriteAsync("update barber.finance_delinquency_cases set assigned_to=@assigned,status='Negotiating',notes=@notes,updated_at=now() where id=@id and tenant_id=@tenant and branch_id=@branch and status='Open'","Finance360.DelinquencyAssigned","finance_delinquency_cases",id,r.Notes,c=>{TeamDataService.Add(c,"assigned",r.AssignedTo);TeamDataService.Add(c,"notes",r.Notes);},ct);return Ok(OkData(new{id}));}[HttpPost("{id:guid}/close"),RequirePermission("Finance360.Delinquency.Manage")]public async Task<IActionResult> Close(Guid id,CloseRequest r,CancellationToken ct){if(string.IsNullOrWhiteSpace(r.Reason))return ValidationProblem("Motivo obrigatório.");await d.WriteAsync("update barber.finance_delinquency_cases set status=@status,close_reason=@reason,closed_at=now(),updated_at=now() where id=@id and tenant_id=@tenant and branch_id=@branch and status in ('Open','Negotiating')","Finance360.DelinquencyClosed","finance_delinquency_cases",id,r.Reason,c=>{TeamDataService.Add(c,"status",r.Status);TeamDataService.Add(c,"reason",r.Reason);},ct);return Ok(OkData(new{id}));}public sealed record AssignRequest(Guid AssignedTo,string? Notes);public sealed record CloseRequest(string Status,string Reason);}
[Route("api/finance360/reports")] public sealed class FinanceReportsController(FinanceAuditService audit):Finance360ControllerBase {[HttpGet("export"),RequirePermission("Finance360.Reports.Export")]public async Task<IActionResult> Export([FromQuery]DateOnly from,[FromQuery]DateOnly to,CancellationToken ct){var x=await audit.ExportAsync(new(from,to),ct);return File(x.Content,"text/csv",x.FileName);}}

[Route("api/mobile/finance360")]
public sealed class MobileFinance360Controller(TeamDataService d):Finance360ControllerBase
{
 [HttpGet("summary"),RequirePermission("Finance360.Read")] public async Task<IActionResult> Summary(CancellationToken ct)=>Ok(OkData((await d.QueryAsync("select coalesce(sum(amount-paid_amount) filter(where status in ('Open','PartiallyPaid','Overdue')),0) receivables from barber.finance_receivables where tenant_id=@tenant and branch_id=@branch",null,ct)).Single()));
 [HttpGet("receivables"),RequirePermission("Finance360.Read")] public async Task<IActionResult> Receivables(CancellationToken ct)=>Ok(OkData(await d.QueryAsync("select description,amount,paid_amount,due_at,status from barber.finance_receivables where tenant_id=@tenant and branch_id=@branch order by due_at limit 50",null,ct)));
 [HttpGet("payables"),RequirePermission("Finance360.Read")] public async Task<IActionResult> Payables(CancellationToken ct)=>Ok(OkData(await d.QueryAsync("select description,amount,paid_amount,due_at,status from barber.finance_payables where tenant_id=@tenant and branch_id=@branch order by due_at limit 50",null,ct)));
 [HttpGet("commissions"),RequirePermission("Finance360.Commissions.Read")] public async Task<IActionResult> Commissions(CancellationToken ct)=>Ok(OkData(await d.QueryAsync("select amount,status,created_at from barber.commissions where tenant_id=@tenant and branch_id=@branch order by created_at desc limit 50",null,ct)));
 [HttpGet("payroll"),RequirePermission("Finance360.Read")] public async Task<IActionResult> Payroll(CancellationToken ct)=>Ok(OkData(await d.QueryAsync("select description,amount,paid_amount,due_at,status from barber.finance_payables where tenant_id=@tenant and branch_id=@branch and source_type='PayrollSettlement' order by due_at desc limit 50",null,ct)));
}
