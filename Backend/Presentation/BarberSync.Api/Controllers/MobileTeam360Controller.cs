using BarberSync.Api.Services.Team;
using BarberSync.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.Api.Controllers;
[ApiController,Authorize,Route("api/mobile/team360")]
public sealed class MobileTeam360Controller(TeamDataService data,ICurrentUserContext current):ControllerBase
{
 private void User(System.Data.Common.DbCommand c)=>TeamDataService.Add(c,"user",current.UserId);
 [HttpGet("me")] public async Task<IActionResult> Me(CancellationToken ct)=>Ok(await data.QueryAsync("select id,display_name,professional_type,status from barber.team_professional_profiles where tenant_id=@tenant and branch_id=@branch and user_id=@user and deleted_at is null",User,ct));
 [HttpGet("schedule")] public Task<IActionResult> Schedule(CancellationToken ct)=>Get("select s.* from barber.team_shifts s join barber.team_professional_profiles p on p.id=s.professional_id where s.tenant_id=@tenant and s.branch_id=@branch and p.user_id=@user and s.starts_at>=now() order by s.starts_at",ct);
 [HttpGet("productivity")] public Task<IActionResult> Productivity(CancellationToken ct)=>Get("select s.* from barber.team_productivity_snapshots s join barber.team_professional_profiles p on p.id=s.professional_id where s.tenant_id=@tenant and s.branch_id=@branch and p.user_id=@user order by s.snapshot_date desc limit 90",ct);
 [HttpGet("commissions")] public Task<IActionResult> Commissions(CancellationToken ct)=>Get("select s.period_start,s.period_end,s.status,s.gross_commission,s.adjustments,s.net_commission from barber.team_commission_settlements s join barber.team_professional_profiles p on p.id=s.professional_id where s.tenant_id=@tenant and s.branch_id=@branch and p.user_id=@user order by s.created_at desc",ct);
 [HttpGet("goals")] public Task<IActionResult> Goals(CancellationToken ct)=>Get("select g.goal_type,g.period_start,g.period_end,g.target_value,g.status from barber.team_goals g join barber.team_professional_profiles p on p.id=g.professional_id where g.tenant_id=@tenant and g.branch_id=@branch and p.user_id=@user order by g.period_start desc",ct);
 [HttpGet("trainings")] public Task<IActionResult> Trainings(CancellationToken ct)=>Get("select e.id,t.title,t.training_type,e.status,e.enrolled_at,e.completed_at from barber.team_training_enrollments e join barber.team_trainings t on t.id=e.training_id join barber.team_professional_profiles p on p.id=e.professional_id where e.tenant_id=@tenant and e.branch_id=@branch and p.user_id=@user order by e.enrolled_at desc",ct);
 [HttpPost("trainings/{id:guid}/complete")] public async Task<IActionResult> Complete(Guid id,CancellationToken ct){await data.WriteAsync("update barber.team_training_enrollments e set status='Completed',completed_at=now(),updated_at=now() from barber.team_professional_profiles p where e.id=@id and e.tenant_id=@tenant and e.branch_id=@branch and p.id=e.professional_id and p.user_id=@user and e.status in ('Enrolled','InProgress')","Team360.MobileTrainingCompleted","team_training_enrollments",id,null,User,ct);return Ok(new{id,status="Completed"});}
 private async Task<IActionResult> Get(string sql,CancellationToken ct)=>Ok(await data.QueryAsync(sql,User,ct));
}
