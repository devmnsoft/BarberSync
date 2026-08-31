using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Text.Json;
using BarberSync.Api.Security;
using BarberSync.Api.Services.Team;
using BarberSync.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Authorize]
public abstract class AnalyticsControllerBase(TeamDataService data, ICurrentUserContext user) : ControllerBase
{
    protected TeamDataService Data { get; } = data;
    protected ICurrentUserContext UserContext { get; } = user;
    protected IActionResult Invalid(string field, string message) => BadRequest(new { message = "Revise os filtros informados.", traceId = HttpContext.TraceIdentifier, errors = new Dictionary<string, string[]> { [field] = [message] } });
    protected static void Add(DbCommand command, string name, object? value) => TeamDataService.Add(command, name, value);

    protected IActionResult? Validate(AnalyticsFilter filter, bool datesRequired = false)
    {
        if (filter.BranchId is { } branch && branch != UserContext.BranchId) return Invalid("branchId", "A unidade selecionada não pertence ao contexto autenticado.");
        if (datesRequired && (filter.From is null || filter.To is null)) return Invalid(filter.From is null ? "from" : "to", "Informe o início e o fim do período.");
        if (filter.From > filter.To) return Invalid("from", "A data inicial deve ser anterior ou igual à data final.");
        if (filter.From is { } from && filter.To is { } to && to.DayNumber - from.DayNumber > 366) return Invalid("to", "O período máximo para consulta é de 366 dias.");
        return null;
    }

    protected (DateOnly From, DateOnly To, DateOnly PreviousFrom, DateOnly PreviousTo) Period(AnalyticsFilter filter)
    {
        var to = filter.To ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var from = filter.From ?? to.AddDays(-29);
        var days = to.DayNumber - from.DayNumber + 1;
        return (from, to, from.AddDays(-days), from.AddDays(-1));
    }
}

public sealed record AnalyticsFilter(DateOnly? From, DateOnly? To, Guid? BranchId, Guid? ProfessionalId, Guid? ClientId, Guid? ServiceId, Guid? ProductId, Guid? SupplierId, Guid? CategoryId, string? Status, string? Scope);

[Route("api/analytics")]
public sealed class AnalyticsController(TeamDataService data, ICurrentUserContext user) : AnalyticsControllerBase(data, user)
{
    private const string CoreSql = @"select
 coalesce(sum(p.amount) filter(where p.status in ('Paid','Confirmed')),0) revenue_total,
 coalesce(sum(p.amount) filter(where p.status in ('Paid','Confirmed')),0)-coalesce((select sum(a.amount) from barber.accounts_payable a where a.tenant_id=@tenant and a.branch_id=@branch and a.status='Paid' and a.paid_at::date between @from and @to),0) revenue_net,
 coalesce((select sum(a.amount) from barber.accounts_payable a where a.tenant_id=@tenant and a.branch_id=@branch and a.status='Paid' and a.paid_at::date between @from and @to),0) expenses,
 count(distinct p.service_order_id) filter(where p.status in ('Paid','Confirmed')) attendances,
 case when count(distinct p.service_order_id) filter(where p.status in ('Paid','Confirmed'))=0 then 0 else coalesce(sum(p.amount) filter(where p.status in ('Paid','Confirmed')),0)/count(distinct p.service_order_id) filter(where p.status in ('Paid','Confirmed')) end average_ticket,
 (select count(*) from barber.clients c where c.tenant_id=@tenant and c.branch_id=@branch and c.deleted_at is null and c.created_at::date between @from and @to) new_clients,
 (select count(*) from barber.appointments a where a.tenant_id=@tenant and a.branch_id=@branch and a.deleted_at is null and a.status='NoShow' and a.scheduled_start::date between @from and @to) no_shows,
 (select count(*) from barber.appointments a where a.tenant_id=@tenant and a.branch_id=@branch and a.deleted_at is null and a.scheduled_start::date between @from and @to) appointments,
 (select count(*) from barber.products x where x.tenant_id=@tenant and x.branch_id=@branch and x.deleted_at is null and x.current_stock<=x.minimum_stock) critical_stock,
 (select coalesce(sum(c.amount),0) from barber.commissions c where c.tenant_id=@tenant and c.branch_id=@branch and c.status in ('Pending','Available')) open_commissions,
 (select coalesce(sum(l.points),0) from barber.loyalty_transactions l where l.tenant_id=@tenant and l.branch_id=@branch and l.type in ('Accrual','Credit') and l.created_at::date between @from and @to) loyalty_issued
from barber.payments p where p.tenant_id=@tenant and p.branch_id=@branch and p.deleted_at is null and coalesce(p.paid_at,p.created_at)::date between @from and @to";

    [HttpGet("executive"), RequirePermission("Analytics.Read")]
    public async Task<IActionResult> Executive([FromQuery] AnalyticsFilter filter, CancellationToken ct)
    {
        if (Validate(filter) is { } error) return error;
        var period = Period(filter);
        var current = (await Data.QueryAsync(CoreSql, c => BindPeriod(c, period.From, period.To), ct)).Single();
        var previous = (await Data.QueryAsync(CoreSql, c => BindPeriod(c, period.PreviousFrom, period.PreviousTo), ct)).Single();
        return Ok(new { success = true, data = new { scope = "Executive", period = new { period.From, period.To }, current, previous, sourceStatus = "Available", generatedAt = DateTimeOffset.UtcNow } });
    }

    [HttpGet("operations"), RequirePermission("Analytics.Read")]
    public Task<IActionResult> Operations([FromQuery] AnalyticsFilter filter, CancellationToken ct) => Scope("Operations", @"select
 count(*) filter(where status='Scheduled') scheduled, count(*) filter(where checked_in_at is not null) check_ins,
 count(*) filter(where status in ('Completed','Finished')) completed, count(*) filter(where status='Cancelled') cancellations,
 count(*) filter(where status='NoShow') no_shows,
 coalesce(round(avg(extract(epoch from (completed_at-started_at))/60) filter(where completed_at is not null and started_at is not null),1),0) average_service_minutes,
 (select count(*) from barber.service_orders o where o.tenant_id=@tenant and o.branch_id=@branch and o.status='Open') open_orders,
 (select count(*) from barber.payments p where p.tenant_id=@tenant and p.branch_id=@branch and p.status='Pending') pending_payments
from barber.appointments where tenant_id=@tenant and branch_id=@branch and deleted_at is null and scheduled_start::date between @from and @to", filter, ct);

    [HttpGet("finance"), RequirePermission("Analytics.Read")]
    public Task<IActionResult> Finance([FromQuery] AnalyticsFilter filter, CancellationToken ct) => Scope("Finance", @"select
 coalesce(sum(amount) filter(where status in ('Paid','Confirmed')),0) cash_in,
 (select coalesce(sum(amount),0) from barber.accounts_payable where tenant_id=@tenant and branch_id=@branch and status='Paid' and paid_at::date between @from and @to) cash_out,
 (select coalesce(sum(amount),0) from barber.accounts_payable where tenant_id=@tenant and branch_id=@branch and status in ('Open','Overdue')) open_payables,
 (select coalesce(sum(amount),0) from barber.accounts_receivable where tenant_id=@tenant and branch_id=@branch and status in ('Open','Overdue')) open_receivables,
 (select coalesce(sum(amount),0) from barber.accounts_receivable where tenant_id=@tenant and branch_id=@branch and status in ('Open','Overdue') and due_date<current_date) overdue_receivables,
 (select count(*) from barber.financial_reconciliations where tenant_id=@tenant and branch_id=@branch and status='Draft') pending_reconciliations
from barber.payments where tenant_id=@tenant and branch_id=@branch and deleted_at is null and coalesce(paid_at,created_at)::date between @from and @to", filter, ct);

    [HttpGet("team"), RequirePermission("Analytics.Read")]
    public Task<IActionResult> Team([FromQuery] AnalyticsFilter filter, CancellationToken ct) => Scope("Team", @"select
 coalesce(sum(amount),0) commissions_generated, coalesce(sum(amount) filter(where status='Paid'),0) commissions_paid,
 count(distinct professional_id) professionals_with_commission,
 (select count(*) from barber.professional_goals where tenant_id=@tenant and branch_id=@branch and status='Achieved') goals_achieved,
 (select count(*) from barber.professional_goals where tenant_id=@tenant and branch_id=@branch and status='Active') goals_active
from barber.commissions where tenant_id=@tenant and branch_id=@branch and created_at::date between @from and @to", filter, ct);

    [HttpGet("relationship"), RequirePermission("Analytics.Read")]
    public Task<IActionResult> Relationship([FromQuery] AnalyticsFilter filter, CancellationToken ct) => Scope("Relationship", @"select
 count(*) filter(where status='Active') active_clients, count(*) filter(where status<>'Active') inactive_clients,
 count(*) filter(where created_at::date between @from and @to) new_clients,
 (select count(*) from barber.coupons where tenant_id=@tenant and branch_id=@branch and status='Active') active_coupons,
 (select count(*) from barber.packages where tenant_id=@tenant and branch_id=@branch and status='Active') active_packages,
 (select coalesce(sum(points),0) from barber.loyalty_transactions where tenant_id=@tenant and branch_id=@branch and type in ('Accrual','Credit') and created_at::date between @from and @to) loyalty_issued,
 (select coalesce(abs(sum(points)),0) from barber.loyalty_transactions where tenant_id=@tenant and branch_id=@branch and type in ('Redemption','Debit') and created_at::date between @from and @to) loyalty_redeemed
from barber.clients where tenant_id=@tenant and branch_id=@branch and deleted_at is null", filter, ct);

    [HttpGet("inventory"), RequirePermission("Analytics.Read")]
    public Task<IActionResult> Inventory([FromQuery] AnalyticsFilter filter, CancellationToken ct) => Scope("Inventory", @"select
 count(*) filter(where current_stock<=minimum_stock) below_minimum, count(*) filter(where current_stock<=0) out_of_stock,
 coalesce(sum(current_stock*cost_price),0) stock_value,
 (select count(*) from barber.purchase_orders where tenant_id=@tenant and branch_id=@branch and status in ('Draft','PendingApproval','Approved','PartiallyReceived')) open_purchases,
 (select count(*) from barber.purchase_receipts where tenant_id=@tenant and branch_id=@branch and status='Draft') pending_receipts,
 (select count(*) from barber.inventory_counts where tenant_id=@tenant and branch_id=@branch and status in ('Draft','Counting')) open_counts,
 (select count(*) from barber.replenishment_suggestions where tenant_id=@tenant and branch_id=@branch and status='Open') replenishment_suggestions
from barber.products where tenant_id=@tenant and branch_id=@branch and deleted_at is null", filter, ct);

    [HttpGet("catalog"), RequirePermission("Analytics.Read")]
    public Task<IActionResult> Catalog([FromQuery] AnalyticsFilter filter, CancellationToken ct) => Scope("Catalog", @"select
 (select coalesce(avg(estimated_margin_percent),0) from barber.catalog_service_profiles where tenant_id=@tenant and branch_id=@branch and status='Active' and deleted_at is null) average_service_margin,
 (select coalesce(avg(margin_percent),0) from barber.catalog_product_profiles where tenant_id=@tenant and branch_id=@branch and status='Active' and deleted_at is null) average_product_margin,
 (select count(*) from barber.catalog_service_profiles where tenant_id=@tenant and branch_id=@branch and estimated_margin_percent<(select coalesce(max(minimum_margin_percent),0) from barber.catalog_margin_rules where tenant_id=@tenant and branch_id=@branch and status='Active') and deleted_at is null) services_below_margin,
 (select count(*) from barber.catalog_product_profiles where tenant_id=@tenant and branch_id=@branch and margin_percent<(select coalesce(max(minimum_margin_percent),0) from barber.catalog_margin_rules where tenant_id=@tenant and branch_id=@branch and status='Active') and deleted_at is null) products_below_margin,
 (select coalesce(sum(base_total-combo_price),0) from barber.catalog_combo_definitions where tenant_id=@tenant and branch_id=@branch and status='Active') combo_discount_value,
 (select count(*) from barber.catalog_package_definitions where tenant_id=@tenant and branch_id=@branch and status='Active') active_packages,
 (select coalesce(sum(commission_amount),0) from barber.catalog_commission_events where tenant_id=@tenant and branch_id=@branch and status in('Payable','Paid') and created_at::date between @from and @to) commissions_generated,
 (select coalesce(sum((result_json->>'discountAmount')::numeric),0) from barber.catalog_price_simulations where tenant_id=@tenant and branch_id=@branch and simulation_type='Price' and created_at::date between @from and @to) simulated_discounts", filter, ct);

    [HttpGet("kpis"), RequirePermission("Analytics.Read")] public Task<IActionResult> Kpis([FromQuery] AnalyticsFilter filter, CancellationToken ct) => Executive(filter, ct);

    [HttpGet("rankings"), RequirePermission("Analytics.Read")]
    public async Task<IActionResult> Rankings([FromQuery] AnalyticsFilter filter, CancellationToken ct)
    {
        if (Validate(filter) is { } error) return error; var p = Period(filter);
        var rows = await Data.QueryAsync(@"select coalesce(pr.name,'Sem profissional') name,count(distinct o.id) attendances,coalesce(sum(pay.amount),0) revenue from barber.service_orders o left join barber.appointments a on a.id=o.appointment_id left join barber.professionals pr on pr.id=a.professional_id left join barber.payments pay on pay.service_order_id=o.id and pay.status in ('Paid','Confirmed') where o.tenant_id=@tenant and o.branch_id=@branch and o.created_at::date between @from and @to group by pr.name order by revenue desc limit 20", c => BindPeriod(c,p.From,p.To), ct);
        return Ok(new { success=true, data=rows, sourceStatus="Available" });
    }

    [HttpGet("filter-options"), RequirePermission("Analytics.Read")]
    public async Task<IActionResult> Options(CancellationToken ct) => Ok(new { success=true, data=new {
        branches=await Data.QueryAsync("select id,name from barber.branches where tenant_id=@tenant and id=@branch and deleted_at is null",null,ct),
        professionals=await Data.QueryAsync("select id,name from barber.professionals where tenant_id=@tenant and branch_id=@branch and deleted_at is null order by name",null,ct),
        clients=await Data.QueryAsync("select id,name from barber.clients where tenant_id=@tenant and branch_id=@branch and deleted_at is null order by name limit 500",null,ct),
        services=await Data.QueryAsync("select id,name from barber.services where tenant_id=@tenant and branch_id=@branch and deleted_at is null order by name",null,ct),
        products=await Data.QueryAsync("select id,name from barber.products where tenant_id=@tenant and branch_id=@branch and deleted_at is null order by name",null,ct),
        suppliers=await Data.QueryAsync("select id,name from barber.suppliers where tenant_id=@tenant and branch_id=@branch and deleted_at is null order by name",null,ct),
        categories=await Data.QueryAsync("select id,name from barber.financial_categories where tenant_id=@tenant and branch_id=@branch and deleted_at is null order by name",null,ct)
    }});

    private async Task<IActionResult> Scope(string scope, string sql, AnalyticsFilter filter, CancellationToken ct)
    { if (Validate(filter) is { } error) return error; var p=Period(filter); var row=(await Data.QueryAsync(sql,c=>BindPeriod(c,p.From,p.To),ct)).Single(); return Ok(new { success=true,data=new { scope,period=new {p.From,p.To},current=row,sourceStatus="Available",generatedAt=DateTimeOffset.UtcNow } }); }
    private static void BindPeriod(DbCommand c, DateOnly from, DateOnly to) { Add(c,"from",from); Add(c,"to",to); }
}

[Route("api/analytics/reports")]
public sealed class AnalyticsReportsController(TeamDataService data, ICurrentUserContext user) : AnalyticsControllerBase(data,user)
{
    private static readonly HashSet<string> Types = ["executive-summary","daily-operations","monthly-finance","dre","team-commissions","client-recurrence","critical-stock","purchases-suppliers","packages-loyalty","reconciliation"];
    [HttpGet("export"), RequirePermission("Analytics.Export")]
    public async Task<IActionResult> Export([FromQuery(Name="type")] string? type, [FromQuery] AnalyticsFilter filter, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(type) || !Types.Contains(type)) return Invalid("type","Selecione um tipo de relatório válido.");
        if (Validate(filter,true) is { } error) return error; var p=Period(filter);
        var rows=await Data.QueryAsync("select coalesce(p.paid_at,p.created_at) occurred_at,p.method,p.status,p.amount,o.number service_order from barber.payments p left join barber.service_orders o on o.id=p.service_order_id where p.tenant_id=@tenant and p.branch_id=@branch and coalesce(p.paid_at,p.created_at)::date between @from and @to order by occurred_at",c=>{Add(c,"from",p.From);Add(c,"to",p.To);},ct);
        var csv=new StringBuilder("data;forma;status;valor;comanda\r\n"); foreach(var row in rows) csv.Append(Csv(row.GetValueOrDefault("occurred_at"))).Append(';').Append(Csv(row.GetValueOrDefault("method"))).Append(';').Append(Csv(row.GetValueOrDefault("status"))).Append(';').Append(Csv(row.GetValueOrDefault("amount"))).Append(';').Append(Csv(row.GetValueOrDefault("service_order"))).Append("\r\n");
        await Data.WriteAsync("insert into barber.analytics_report_exports(id,tenant_id,branch_id,user_id,report_type,filters_json,format,status,file_name,completed_at) values(@id,@tenant,@branch,@user,@type,@filters::jsonb,'CSV','Completed',@file,now())","Analytics.ReportExported","analytics_report_exports",null,null,c=>{Add(c,"user",UserContext.UserId);Add(c,"type",type);Add(c,"filters",JsonSerializer.Serialize(filter));Add(c,"file",$"{type}-{p.From:yyyyMMdd}-{p.To:yyyyMMdd}.csv");},ct);
        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(),"text/csv",$"{type}-{p.From:yyyyMMdd}-{p.To:yyyyMMdd}.csv");
    }
    private static string Csv(object? value) => $"\"{Convert.ToString(value,CultureInfo.InvariantCulture)?.Replace("\"","\"\"")}\"";
}

public sealed record AlertRuleRequest(string Name,string Scope,string MetricKey,string Operator,decimal ThresholdValue,string PeriodType,string Severity,Guid? BranchId,string Status="Active");
public sealed record AlertActionRequest(string? Reason);
[Route("api/analytics")]
public sealed class AnalyticsAlertsController(TeamDataService data, ICurrentUserContext user) : AnalyticsControllerBase(data,user)
{
    private static readonly HashSet<string> Scopes=["Executive","Operations","Finance","Team","Relationship","Inventory"];
    private static readonly HashSet<string> Metrics=["critical_stock","revenue_drop_percent","no_show_rate","overdue_payables","overdue_receivables","pending_commissions","goal_risk","inactive_vip","pending_cashback","late_purchase","pending_reconciliation"];
    [HttpGet("alerts"),RequirePermission("Analytics.Alerts")] public async Task<IActionResult> List(CancellationToken ct)=>Ok(new{success=true,data=await Data.QueryAsync("select e.*,r.name rule_name from barber.analytics_alert_events e left join barber.analytics_alert_rules r on r.id=e.rule_id where e.tenant_id=@tenant and e.branch_id=@branch order by e.created_at desc limit 200",null,ct)});
    [HttpPost("alerts/rules"),RequirePermission("Analytics.Alerts")] public Task<IActionResult> Create(AlertRuleRequest r,CancellationToken ct)=>Save(null,r,ct);
    [HttpPut("alerts/rules/{id:guid}"),RequirePermission("Analytics.Alerts")] public Task<IActionResult> Update(Guid id,AlertRuleRequest r,CancellationToken ct)=>Save(id,r,ct);
    [HttpPost("alerts/{id:guid}/acknowledge"),RequirePermission("Analytics.Alerts")] public Task<IActionResult> Acknowledge(Guid id,AlertActionRequest r,CancellationToken ct)=>Transition(id,"Acknowledged",r.Reason,ct);
    [HttpPost("alerts/{id:guid}/resolve"),RequirePermission("Analytics.Alerts")] public Task<IActionResult> Resolve(Guid id,AlertActionRequest r,CancellationToken ct)=>Transition(id,"Resolved",r.Reason,ct);
    [HttpPost("alerts/{id:guid}/dismiss"),RequirePermission("Analytics.Alerts")] public Task<IActionResult> Dismiss(Guid id,AlertActionRequest r,CancellationToken ct)=>Transition(id,"Dismissed",r.Reason,ct);
    private async Task<IActionResult> Save(Guid? id,AlertRuleRequest r,CancellationToken ct){if(string.IsNullOrWhiteSpace(r.Name))return Invalid("name","Nome é obrigatório.");if(!Scopes.Contains(r.Scope))return Invalid("scope","Escopo inválido.");if(!Metrics.Contains(r.MetricKey))return Invalid("metricKey","Métrica inválida.");if(!new[]{">",">=","<","<=","="}.Contains(r.Operator))return Invalid("operator","Operador inválido.");if(r.ThresholdValue<0)return Invalid("thresholdValue","O limite não pode ser negativo.");if(r.BranchId is{} b&&b!=UserContext.BranchId)return Invalid("branchId","Unidade fora do contexto autenticado.");var key=await Data.WriteAsync("insert into barber.analytics_alert_rules(id,tenant_id,branch_id,name,scope,metric_key,operator,threshold_value,period_type,severity,status,created_by) values(@id,@tenant,@branch,@name,@scope,@metric,@operator,@threshold,@period,@severity,@status,@user) on conflict(id) do update set name=excluded.name,scope=excluded.scope,metric_key=excluded.metric_key,operator=excluded.operator,threshold_value=excluded.threshold_value,period_type=excluded.period_type,severity=excluded.severity,status=excluded.status,updated_at=now() where analytics_alert_rules.tenant_id=@tenant and analytics_alert_rules.branch_id=@branch","Analytics.AlertRuleSaved","analytics_alert_rules",id,null,c=>{Add(c,"name",r.Name.Trim());Add(c,"scope",r.Scope);Add(c,"metric",r.MetricKey);Add(c,"operator",r.Operator);Add(c,"threshold",r.ThresholdValue);Add(c,"period",r.PeriodType);Add(c,"severity",r.Severity);Add(c,"status",r.Status);Add(c,"user",UserContext.UserId);},ct);return Ok(new{success=true,data=new{id=key}});}
    private async Task<IActionResult> Transition(Guid id,string status,string? reason,CancellationToken ct){if(status=="Dismissed"&&string.IsNullOrWhiteSpace(reason))return Invalid("reason","Informe o motivo para dispensar o alerta.");await Data.WriteAsync("update barber.analytics_alert_events set status=@status,acknowledged_by=case when @status='Acknowledged' then @user else acknowledged_by end,acknowledged_at=case when @status='Acknowledged' then now() else acknowledged_at end,resolved_by=case when @status in ('Resolved','Dismissed') then @user else resolved_by end,resolved_at=case when @status in ('Resolved','Dismissed') then now() else resolved_at end,source_json=source_json||jsonb_build_object('actionReason',@reason) where id=@id and tenant_id=@tenant and branch_id=@branch and status in ('Open','Acknowledged')","Analytics.Alert"+status,"analytics_alert_events",id,reason,c=>{Add(c,"status",status);Add(c,"user",UserContext.UserId);Add(c,"reason",reason);},ct);return Ok(new{success=true,data=new{id,status}});}
}

public sealed record SavedViewRequest(string Name,string Scope,JsonElement Filters,bool IsDefault,Guid? BranchId);
[Route("api/analytics/saved-views")]
public sealed class AnalyticsSavedViewsController(TeamDataService data, ICurrentUserContext user) : AnalyticsControllerBase(data,user)
{
    [HttpGet,RequirePermission("Analytics.Read")] public async Task<IActionResult> List(CancellationToken ct)=>Ok(new{success=true,data=await Data.QueryAsync("select id,name,scope,filters_json,is_default,created_at,updated_at from barber.analytics_saved_views where tenant_id=@tenant and branch_id=@branch and user_id=@user and deleted_at is null order by is_default desc,name",c=>Add(c,"user",UserContext.UserId),ct)});
    [HttpPost,RequirePermission("Analytics.Manage")] public Task<IActionResult> Create(SavedViewRequest r,CancellationToken ct)=>Save(null,r,ct);
    [HttpPut("{id:guid}"),RequirePermission("Analytics.Manage")] public Task<IActionResult> Update(Guid id,SavedViewRequest r,CancellationToken ct)=>Save(id,r,ct);
    [HttpDelete("{id:guid}"),RequirePermission("Analytics.Manage")] public async Task<IActionResult> Delete(Guid id,CancellationToken ct){await Data.WriteAsync("update barber.analytics_saved_views set deleted_at=now(),updated_at=now() where id=@id and tenant_id=@tenant and branch_id=@branch and user_id=@user and deleted_at is null","Analytics.SavedViewDeleted","analytics_saved_views",id,null,c=>Add(c,"user",UserContext.UserId),ct);return NoContent();}
    private async Task<IActionResult> Save(Guid? id,SavedViewRequest r,CancellationToken ct){if(string.IsNullOrWhiteSpace(r.Name))return Invalid("name","Nome é obrigatório.");if(!new[]{"Executive","Operations","Finance","Team","Relationship","Inventory"}.Contains(r.Scope))return Invalid("scope","Escopo inválido.");if(r.Filters.ValueKind!=JsonValueKind.Object)return Invalid("filters","Os filtros devem vir das seleções do dashboard.");if(r.BranchId is{} b&&b!=UserContext.BranchId)return Invalid("branchId","Unidade fora do contexto autenticado.");var key=await Data.WriteAsync(@"with reset as(update barber.analytics_saved_views set is_default=false where @default and tenant_id=@tenant and branch_id=@branch and user_id=@user and deleted_at is null) insert into barber.analytics_saved_views(id,tenant_id,branch_id,user_id,name,scope,filters_json,is_default) values(@id,@tenant,@branch,@user,@name,@scope,@filters::jsonb,@default) on conflict(id) do update set name=excluded.name,scope=excluded.scope,filters_json=excluded.filters_json,is_default=excluded.is_default,updated_at=now() where analytics_saved_views.tenant_id=@tenant and analytics_saved_views.branch_id=@branch and analytics_saved_views.user_id=@user","Analytics.SavedViewSaved","analytics_saved_views",id,null,c=>{Add(c,"user",UserContext.UserId);Add(c,"name",r.Name.Trim());Add(c,"scope",r.Scope);Add(c,"filters",r.Filters.GetRawText());Add(c,"default",r.IsDefault);},ct);return Ok(new{success=true,data=new{id=key}});}
}
