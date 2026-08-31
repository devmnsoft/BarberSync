using BarberSync.Api.Security;
using BarberSync.Api.Services.Team;
using BarberSync.Api.Services.Team360;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Authorize, Route("api/team360")]
public sealed class Team360Controller(TeamDataService data, TeamProfileService profiles, TeamScheduleService schedules, TeamGoalService goals, TeamProductivityService productivity, TeamCommissionSettlementService commissions, TeamTrainingService training) : ControllerBase
{
    [HttpGet("dashboard"),RequirePermission("Team360.Read")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)=>Ok(await data.QueryAsync(@"select (select count(*) from barber.team_professional_profiles where tenant_id=@tenant and branch_id=@branch and status='Active' and deleted_at is null) active_professionals,(select count(*) from barber.team_shifts where tenant_id=@tenant and branch_id=@branch and starts_at::date=current_date and status<>'Cancelled') shifts_today,(select count(*) from barber.team_goals where tenant_id=@tenant and branch_id=@branch and status='Active') active_goals,(select count(*) from barber.team_certifications where tenant_id=@tenant and branch_id=@branch and expires_at<current_date and status='Active') expired_certifications",null,ct));
    [HttpGet("filter-options"),RequirePermission("Team360.Read")]
    public async Task<IActionResult> Options(CancellationToken ct)=>Ok(await data.QueryAsync("select id,display_name,status,professional_type from barber.team_professional_profiles where tenant_id=@tenant and branch_id=@branch and deleted_at is null order by display_name",null,ct));
    [HttpGet("professionals"),RequirePermission("Team360.Read")]
    public async Task<IActionResult> Professionals(CancellationToken ct)=>Ok(await data.QueryAsync("select id,display_name,professional_type,status,booking_visible,user_id from barber.team_professional_profiles where tenant_id=@tenant and branch_id=@branch and deleted_at is null order by display_name",null,ct));
    [HttpPost("professionals"),RequirePermission("Team360.Professionals.Manage")] public async Task<IActionResult>Create(TeamProfileRequest r,CancellationToken ct)=>Ok(await profiles.CreateProfessionalProfileAsync(r,ct));
    [HttpPut("professionals/{id:guid}"),RequirePermission("Team360.Professionals.Manage")] public async Task<IActionResult>Update(Guid id,TeamProfileRequest r,CancellationToken ct)=>Ok(await profiles.UpdateProfessionalProfileAsync(r with{ProfessionalId=id},ct));
    [HttpPost("professionals/{id:guid}/link-user"),RequirePermission("Team360.Professionals.Manage")] public async Task<IActionResult>Link(Guid id,TeamProfileRequest r,CancellationToken ct)=>Ok(await profiles.LinkUserAsync(r with{ProfessionalId=id},ct));
    [HttpPost("professionals/{id:guid}/{status:regex(activate|suspend|archive)}"),RequirePermission("Team360.Professionals.Manage")] public async Task<IActionResult>Status(Guid id,string status,TeamProfileRequest r,CancellationToken ct)=>Ok(status switch{"activate"=>await profiles.ActivateProfessionalAsync(r with{ProfessionalId=id},ct),"suspend"=>await profiles.SuspendProfessionalAsync(r with{ProfessionalId=id},ct),_=>await profiles.ArchiveProfessionalAsync(r with{ProfessionalId=id},ct)});
    [HttpGet("availability"),HttpGet("shifts"),HttpGet("absences"),HttpGet("vacations"),RequirePermission("Team360.Read")] public async Task<IActionResult>Periods(CancellationToken ct)=>Ok(await data.QueryAsync($"select * from barber.team_{(Request.Path.Value?.Split('/').Last()??"shifts")} where tenant_id=@tenant and branch_id=@branch order by starts_at desc",null,ct));
    [HttpPost("availability"),RequirePermission("Team360.Schedules.Manage")] public async Task<IActionResult>Availability(TeamPeriodRequest r,CancellationToken ct)=>Ok(await schedules.SetAvailabilityAsync(r,ct));
    [HttpPost("shifts"),RequirePermission("Team360.Schedules.Manage")] public async Task<IActionResult>Shift(TeamPeriodRequest r,CancellationToken ct)=>Ok(await schedules.CreateShiftAsync(r,ct));
    [HttpPut("shifts/{id:guid}"),RequirePermission("Team360.Schedules.Manage")] public async Task<IActionResult>Shift(Guid id,TeamPeriodRequest r,CancellationToken ct)=>Ok(await schedules.UpdateShiftAsync(r,ct));
    [HttpPost("absences"),RequirePermission("Team360.Absences.Manage")] public async Task<IActionResult>Absence(TeamPeriodRequest r,CancellationToken ct)=>Ok(await schedules.RegisterAbsenceAsync(r,ct));
    [HttpPost("vacations"),RequirePermission("Team360.Absences.Manage")] public async Task<IActionResult>Vacation(TeamPeriodRequest r,CancellationToken ct)=>Ok(await schedules.RegisterVacationAsync(r,ct));
    [HttpGet("goals"),RequirePermission("Team360.Read")] public async Task<IActionResult>Goals(CancellationToken ct)=>Ok(await data.QueryAsync("select * from barber.team_goals where tenant_id=@tenant and branch_id=@branch order by period_start desc",null,ct));
    [HttpPost("goals"),RequirePermission("Team360.Goals.Manage")] public async Task<IActionResult>Goal(TeamGoalRequest r,CancellationToken ct)=>Ok(await goals.CreateGoalAsync(r,ct));
    [HttpGet("goals/progress"),RequirePermission("Team360.Read")] public async Task<IActionResult>Progress([FromQuery]DateOnly from,[FromQuery]DateOnly to,CancellationToken ct)=>Ok(await goals.GetGoalProgressAsync(new(null,from,to),ct));
    [HttpGet("productivity"),RequirePermission("Team360.Productivity.Read")] public async Task<IActionResult>Productivity([FromQuery]DateOnly from,[FromQuery]DateOnly to,CancellationToken ct)=>Ok(await productivity.GetDashboardAsync(new(null,from,to),ct));
    [HttpGet("productivity/{professionalId:guid}"),RequirePermission("Team360.Productivity.Read")] public async Task<IActionResult>Productivity(Guid professionalId,[FromQuery]DateOnly from,[FromQuery]DateOnly to,CancellationToken ct)=>Ok(await productivity.GetProfessionalProductivityAsync(new(professionalId,from,to),ct));
    [HttpPost("commissions/preview-settlement"),RequirePermission("Team360.Commissions.Manage")] public async Task<IActionResult>Preview(TeamCommissionRequest r,CancellationToken ct)=>Ok(await commissions.PreviewSettlementAsync(r,ct));
    [HttpPost("commissions/settlements"),RequirePermission("Team360.Commissions.Manage")] public async Task<IActionResult>Settlement(TeamCommissionRequest r,CancellationToken ct)=>Ok(await commissions.CreateSettlementAsync(r,ct));
    [HttpPost("commissions/settlements/{id:guid}/approve"),RequirePermission("Team360.Commissions.Manage")] public async Task<IActionResult>Approve(Guid id,CancellationToken ct)=>Ok(await commissions.ApproveSettlementAsync(id,ct));
    [HttpPost("commissions/settlements/{id:guid}/mark-paid"),RequirePermission("Team360.Payroll.Manage")] public async Task<IActionResult>Paid(Guid id,PaymentLink r,CancellationToken ct)=>Ok(await commissions.MarkSettlementPaidAsync(id,r.PaymentId,r.ManualAuthorized,ct));
    [HttpPost("commissions/settlements/{id:guid}/reverse"),RequirePermission("Team360.Commissions.Manage")] public async Task<IActionResult>Reverse(Guid id,ReasonRequest r,CancellationToken ct)=>Ok(await commissions.ReverseSettlementAsync(id,r.Reason,ct));
    [HttpGet("trainings"),RequirePermission("Team360.Read")] public async Task<IActionResult>Trainings(CancellationToken ct)=>Ok(await data.QueryAsync("select * from barber.team_trainings where tenant_id=@tenant and branch_id=@branch and deleted_at is null order by created_at desc",null,ct));
    [HttpPost("trainings"),RequirePermission("Team360.Training.Manage")] public async Task<IActionResult>Training(TeamTrainingRequest r,CancellationToken ct)=>Ok(await training.CreateTrainingAsync(r,ct));
    [HttpGet("audit"),RequirePermission("Team360.Read")] public async Task<IActionResult>Audit(CancellationToken ct)=>Ok(await data.QueryAsync("select * from barber.team_audit_events where tenant_id=@tenant and branch_id=@branch order by created_at desc limit 250",null,ct));
    [HttpGet("reports/export"),RequirePermission("Team360.Reports.Export")] public async Task<IActionResult>Export([FromQuery]DateOnly from,[FromQuery]DateOnly to,CancellationToken ct)=>File(await productivity.ExportProductivityAsync(new(null,from,to),ct),"text/csv","team360-productivity.csv");
    public sealed record PaymentLink(string PaymentId,bool ManualAuthorized=false); public sealed record ReasonRequest(string Reason);
}
