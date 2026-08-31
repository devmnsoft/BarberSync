using BarberSync.Api.Services.Team;

namespace BarberSync.Api.Services.Team360;

public sealed record TeamContext(Guid ProfessionalId, Guid? RoleId = null);
public sealed record TeamProfileRequest(Guid ProfessionalId, string DisplayName, string ProfessionalType, string Status, string? Reason = null, Guid? UserId = null, bool BookingVisible = true);
public sealed record TeamProfileResult(Guid Id, string Status);
public sealed record TeamPeriodRequest(Guid ProfessionalId, DateTimeOffset StartsAt, DateTimeOffset EndsAt, string Status = "Requested", string? Reason = null);
public sealed record TeamScheduleResult(Guid Id, string Status);
public sealed record TeamGoalRequest(Guid? ProfessionalId, Guid? RoleId, string GoalType, DateOnly PeriodStart, DateOnly PeriodEnd, decimal TargetValue, string Status = "Draft");
public sealed record TeamGoalResult(Guid Id, string Status);
public sealed record TeamProductivityRequest(Guid? ProfessionalId, DateOnly From, DateOnly To);
public sealed record TeamCommissionRequest(Guid ProfessionalId, DateOnly PeriodStart, DateOnly PeriodEnd, decimal Adjustments = 0, string? Reason = null);
public sealed record TeamSettlementResult(Guid Id, string Status);
public sealed record TeamTrainingRequest(string Title, string TrainingType, Guid? MandatoryForRoleId = null, string? Description = null);
public sealed record TeamTrainingResult(Guid Id, string Status);

public sealed class TeamProfileService(TeamDataService data)
{
    public Task<TeamProfileResult> CreateProfessionalProfileAsync(TeamProfileRequest request, CancellationToken ct) => Save(request, null, ct);
    public Task<TeamProfileResult> UpdateProfessionalProfileAsync(TeamProfileRequest request, CancellationToken ct) => Save(request, request.ProfessionalId, ct);
    public async Task<TeamProfileResult> LinkUserAsync(TeamProfileRequest request, CancellationToken ct)
    {
        if (request.UserId is null) throw new ArgumentException("Selecione um usuário válido.");
        await data.WriteAsync("update barber.team_professional_profiles p set user_id=u.id,updated_at=now() from barber.users u where p.id=@id and p.tenant_id=@tenant and p.branch_id=@branch and u.id=@user and u.tenant_id=@tenant and u.deleted_at is null", "Team360.UserLinked", "team_professional_profiles", request.ProfessionalId, request.Reason, c => TeamDataService.Add(c,"user",request.UserId), ct);
        return new(request.ProfessionalId, request.Status);
    }
    public Task<TeamProfileResult> ActivateProfessionalAsync(TeamProfileRequest r,CancellationToken ct)=>ChangeStatus(r,"Active",ct);
    public Task<TeamProfileResult> SuspendProfessionalAsync(TeamProfileRequest r,CancellationToken ct)=>ChangeStatus(r,"Suspended",ct);
    public Task<TeamProfileResult> ArchiveProfessionalAsync(TeamProfileRequest r,CancellationToken ct)=>ChangeStatus(r,"Archived",ct);
    private async Task<TeamProfileResult> Save(TeamProfileRequest r,Guid? id,CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(r.DisplayName)||string.IsNullOrWhiteSpace(r.ProfessionalType)) throw new ArgumentException("Nome e tipo profissional são obrigatórios.");
        var key=await data.WriteAsync(@"insert into barber.team_professional_profiles(id,tenant_id,branch_id,user_id,display_name,professional_type,status,booking_visible,created_by) values(@id,@tenant,@branch,@user,@name,@type,@status,@booking,@actor) on conflict(id) do update set display_name=excluded.display_name,professional_type=excluded.professional_type,booking_visible=excluded.booking_visible,updated_at=now() where team_professional_profiles.tenant_id=@tenant and team_professional_profiles.branch_id=@branch", "Team360.ProfileSaved","team_professional_profiles",id,r.Reason,c=>{TeamDataService.Add(c,"user",r.UserId);TeamDataService.Add(c,"name",r.DisplayName.Trim());TeamDataService.Add(c,"type",r.ProfessionalType);TeamDataService.Add(c,"status",r.Status);TeamDataService.Add(c,"booking",r.BookingVisible);TeamDataService.Add(c,"actor",Guid.Empty);},ct);
        return new(key,r.Status);
    }
    private async Task<TeamProfileResult> ChangeStatus(TeamProfileRequest r,string status,CancellationToken ct){if(string.IsNullOrWhiteSpace(r.Reason))throw new ArgumentException("Motivo é obrigatório para alterar o status.");await data.WriteAsync("update barber.team_professional_profiles set status=@status,booking_visible=case when @status='Active' then booking_visible else false end,updated_at=now() where id=@id and tenant_id=@tenant and branch_id=@branch and deleted_at is null","Team360.StatusChanged","team_professional_profiles",r.ProfessionalId,r.Reason,c=>TeamDataService.Add(c,"status",status),ct);return new(r.ProfessionalId,status);}
}

public sealed class TeamScheduleService(TeamDataService data)
{
    public Task<TeamScheduleResult> SetAvailabilityAsync(TeamPeriodRequest r,CancellationToken ct)=>InsertPeriod("team_availability_rules",r,"Active",ct);
    public async Task<TeamScheduleResult> CreateShiftAsync(TeamPeriodRequest r,CancellationToken ct){await EnsureNoConflict(r,ct);return await InsertPeriod("team_shifts",r,"Scheduled",ct);}
    public async Task<TeamScheduleResult> UpdateShiftAsync(TeamPeriodRequest r,CancellationToken ct){await EnsureNoConflict(r,ct);return await InsertPeriod("team_shifts",r,r.Status,ct);}
    public Task<TeamScheduleResult> RegisterAbsenceAsync(TeamPeriodRequest r,CancellationToken ct)=>InsertPeriod("team_absences",r,r.Status,ct);
    public Task<TeamScheduleResult> RegisterVacationAsync(TeamPeriodRequest r,CancellationToken ct)=>InsertPeriod("team_vacations",r,r.Status,ct);
    public async Task<bool> ValidateScheduleConflictsAsync(TeamPeriodRequest r,CancellationToken ct)=>(long)(await data.ScalarAsync(@"select count(*) from (select starts_at,ends_at from barber.team_absences where tenant_id=@tenant and branch_id=@branch and professional_id=@professional and status='Approved' union all select starts_at,ends_at from barber.team_vacations where tenant_id=@tenant and branch_id=@branch and professional_id=@professional and status='Approved') x where x.starts_at<@ends and x.ends_at>@starts",c=>Bind(c,r),ct)??0L)>0;
    private async Task EnsureNoConflict(TeamPeriodRequest r,CancellationToken ct){if(r.EndsAt<=r.StartsAt)throw new ArgumentException("O fim deve ser posterior ao início.");if(await ValidateScheduleConflictsAsync(r,ct))throw new InvalidOperationException("A escala conflita com ausência ou férias aprovadas.");}
    private async Task<TeamScheduleResult> InsertPeriod(string table,TeamPeriodRequest r,string status,CancellationToken ct){if(r.EndsAt<=r.StartsAt)throw new ArgumentException("Período inválido.");var key=await data.WriteAsync($"insert into barber.{table}(id,tenant_id,branch_id,professional_id,starts_at,ends_at,status,reason,created_by) values(@id,@tenant,@branch,@professional,@starts,@ends,@status,@reason,@actor)","Team360.ScheduleChanged",table,null,r.Reason,c=>{Bind(c,r);TeamDataService.Add(c,"status",status);TeamDataService.Add(c,"reason",r.Reason);TeamDataService.Add(c,"actor",Guid.Empty);},ct);return new(key,status);}
    private static void Bind(System.Data.Common.DbCommand c,TeamPeriodRequest r){TeamDataService.Add(c,"professional",r.ProfessionalId);TeamDataService.Add(c,"starts",r.StartsAt);TeamDataService.Add(c,"ends",r.EndsAt);}
}

public sealed class TeamGoalService(TeamDataService data)
{
    public Task<TeamGoalResult> CreateGoalAsync(TeamGoalRequest r,CancellationToken ct)=>Save(r,null,ct);
    public Task<TeamGoalResult> UpdateGoalAsync(TeamGoalRequest r,CancellationToken ct)=>Save(r,r.ProfessionalId,ct);
    public Task<IReadOnlyList<Dictionary<string,object?>>> GetGoalProgressAsync(TeamProductivityRequest r,CancellationToken ct)=>data.QueryAsync("select g.*,coalesce(s.current_value,0) current_value from barber.team_goals g left join lateral(select current_value from barber.team_goal_progress_snapshots where goal_id=g.id order by snapshot_date desc limit 1)s on true where g.tenant_id=@tenant and g.branch_id=@branch and g.period_start<=@to and g.period_end>=@from",c=>{TeamDataService.Add(c,"from",r.From);TeamDataService.Add(c,"to",r.To);},ct);
    public async Task<TeamGoalResult> CloseGoalAsync(TeamGoalRequest r,CancellationToken ct){if(r.ProfessionalId is null)throw new ArgumentException("Selecione a meta.");await data.WriteAsync("update barber.team_goals set status='Closed',closed_at=now(),updated_at=now() where id=@id and tenant_id=@tenant and branch_id=@branch and status='Active'","Team360.GoalClosed","team_goals",r.ProfessionalId,null,null,ct);return new(r.ProfessionalId.Value,"Closed");}
    private async Task<TeamGoalResult> Save(TeamGoalRequest r,Guid? id,CancellationToken ct){if(r.PeriodEnd<r.PeriodStart||r.TargetValue<=0||string.IsNullOrWhiteSpace(r.GoalType))throw new ArgumentException("Período, métrica e alvo válidos são obrigatórios.");var key=await data.WriteAsync(@"insert into barber.team_goals(id,tenant_id,branch_id,professional_id,team_role_id,goal_type,period_start,period_end,target_value,status,created_by) values(@id,@tenant,@branch,@professional,@role,@type,@start,@end,@target,@status,@actor) on conflict(id) do update set target_value=excluded.target_value,status=excluded.status,updated_at=now() where team_goals.tenant_id=@tenant and team_goals.branch_id=@branch","Team360.GoalSaved","team_goals",id,null,c=>{TeamDataService.Add(c,"professional",r.ProfessionalId);TeamDataService.Add(c,"role",r.RoleId);TeamDataService.Add(c,"type",r.GoalType);TeamDataService.Add(c,"start",r.PeriodStart);TeamDataService.Add(c,"end",r.PeriodEnd);TeamDataService.Add(c,"target",r.TargetValue);TeamDataService.Add(c,"status",r.Status);TeamDataService.Add(c,"actor",Guid.Empty);},ct);return new(key,r.Status);}
}

public sealed class TeamProductivityService(TeamDataService data)
{
    public Task<IReadOnlyList<Dictionary<string,object?>>> GetDashboardAsync(TeamProductivityRequest r,CancellationToken ct)=>Query(r,ct);
    public Task<IReadOnlyList<Dictionary<string,object?>>> GetProfessionalProductivityAsync(TeamProductivityRequest r,CancellationToken ct)=>Query(r,ct);
    public async Task<byte[]> ExportProductivityAsync(TeamProductivityRequest r,CancellationToken ct){var rows=await Query(r,ct);var lines=new List<string>{"professional_id;appointments;services;revenue;ticket_average;commission;nps;source_status"};lines.AddRange(rows.Select(x=>string.Join(';',new[]{"professional_id","appointments_count","completed_services_count","revenue_amount","ticket_average","commission_amount","nps_score","source_status"}.Select(k=>x.GetValueOrDefault(k)?.ToString()?.Replace(';',',')??""))));return System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine,lines));}
    private Task<IReadOnlyList<Dictionary<string,object?>>> Query(TeamProductivityRequest r,CancellationToken ct)=>data.QueryAsync(@"select p.id professional_id,p.display_name,coalesce(sum(s.appointments_count),0) appointments_count,coalesce(sum(s.completed_services_count),0) completed_services_count,coalesce(sum(s.revenue_amount),0) revenue_amount,case when sum(s.completed_services_count)>0 then sum(s.revenue_amount)/sum(s.completed_services_count) else 0 end ticket_average,coalesce(sum(s.commission_amount),0) commission_amount,avg(s.nps_score) nps_score,case when count(s.id)=0 then 'Unavailable' else 'Available' end source_status from barber.team_professional_profiles p left join barber.team_productivity_snapshots s on s.professional_id=p.id and s.snapshot_date between @from and @to where p.tenant_id=@tenant and p.branch_id=@branch and (@professional is null or p.id=@professional) group by p.id,p.display_name order by p.display_name",c=>{TeamDataService.Add(c,"professional",r.ProfessionalId);TeamDataService.Add(c,"from",r.From);TeamDataService.Add(c,"to",r.To);},ct);
}

public sealed class TeamCommissionSettlementService(TeamDataService data)
{
    public Task<IReadOnlyList<Dictionary<string,object?>>> PreviewSettlementAsync(TeamCommissionRequest r,CancellationToken ct)=>data.QueryAsync("select count(*) item_count,coalesce(sum(amount),0) gross_commission,coalesce(sum(amount),0)-@adjustment net_commission from barber.commissions where tenant_id=@tenant and branch_id=@branch and professional_id=@professional and status='Available' and created_at::date between @from and @to",c=>Bind(c,r),ct);
    public async Task<TeamSettlementResult> CreateSettlementAsync(TeamCommissionRequest r,CancellationToken ct)=>new(await data.CreateSettlement(r.ProfessionalId,r.PeriodStart,r.PeriodEnd,r.Adjustments,r.Reason,ct),"Draft");
    public async Task<TeamSettlementResult> ApproveSettlementAsync(Guid id,CancellationToken ct){await data.MarkSettlement(id,false,null,null,ct);return new(id,"Approved");}
    public async Task<TeamSettlementResult> MarkSettlementPaidAsync(Guid id,string paymentId,bool manualAuthorized,CancellationToken ct){if(string.IsNullOrWhiteSpace(paymentId)&&!manualAuthorized)throw new ArgumentException("Pagamento real ou baixa manual autorizada é obrigatório.");await data.MarkSettlement(id,true,manualAuthorized?"ManualAuthorized":"Payment",paymentId??"manual-authorized",ct);return new(id,"Paid");}
    public async Task<TeamSettlementResult> ReverseSettlementAsync(Guid id,string reason,CancellationToken ct){if(string.IsNullOrWhiteSpace(reason))throw new ArgumentException("Motivo é obrigatório.");await data.WriteAsync("update barber.commission_settlements set status='Reversed',updated_at=now() where id=@id and tenant_id=@tenant and branch_id=@branch and status in ('Approved','Paid')","Team360.SettlementReversed","commission_settlements",id,reason,null,ct);return new(id,"Reversed");}
    private static void Bind(System.Data.Common.DbCommand c,TeamCommissionRequest r){TeamDataService.Add(c,"professional",r.ProfessionalId);TeamDataService.Add(c,"from",r.PeriodStart);TeamDataService.Add(c,"to",r.PeriodEnd);TeamDataService.Add(c,"adjustment",r.Adjustments);}
}

public sealed class TeamTrainingService(TeamDataService data)
{
    public async Task<TeamTrainingResult> CreateTrainingAsync(TeamTrainingRequest r,CancellationToken ct){if(string.IsNullOrWhiteSpace(r.Title))throw new ArgumentException("Título obrigatório.");var id=await data.WriteAsync("insert into barber.team_trainings(id,tenant_id,branch_id,title,description,training_type,mandatory_for_role_id,status,created_by) values(@id,@tenant,@branch,@title,@description,@type,@role,'Active',@actor)","Team360.TrainingCreated","team_trainings",null,null,c=>{TeamDataService.Add(c,"title",r.Title);TeamDataService.Add(c,"description",r.Description);TeamDataService.Add(c,"type",r.TrainingType);TeamDataService.Add(c,"role",r.MandatoryForRoleId);TeamDataService.Add(c,"actor",Guid.Empty);},ct);return new(id,"Active");}
    public Task<TeamTrainingResult> EnrollProfessionalAsync(Guid trainingId,Guid professionalId,CancellationToken ct)=>Enrollment(trainingId,professionalId,"Enrolled",ct);
    public Task<TeamTrainingResult> CompleteTrainingAsync(Guid enrollmentId,Guid responsibleId,CancellationToken ct)=>Enrollment(enrollmentId,responsibleId,"Completed",ct);
    public async Task<TeamTrainingResult> RegisterCertificationAsync(Guid professionalId,Guid? serviceId,string title,DateOnly issuedAt,DateOnly? expiresAt,CancellationToken ct){var id=await data.WriteAsync("insert into barber.team_certifications(id,tenant_id,branch_id,professional_id,service_id,title,issued_at,expires_at,status,created_by) values(@id,@tenant,@branch,@professional,@service,@title,@issued,@expires,'Active',@actor)","Team360.CertificationCreated","team_certifications",null,null,c=>{TeamDataService.Add(c,"professional",professionalId);TeamDataService.Add(c,"service",serviceId);TeamDataService.Add(c,"title",title);TeamDataService.Add(c,"issued",issuedAt);TeamDataService.Add(c,"expires",expiresAt);TeamDataService.Add(c,"actor",Guid.Empty);},ct);return new(id,"Active");}
    private async Task<TeamTrainingResult> Enrollment(Guid a,Guid b,string status,CancellationToken ct){var id=await data.WriteAsync("insert into barber.team_training_enrollments(id,tenant_id,branch_id,training_id,professional_id,status,enrolled_at) values(@id,@tenant,@branch,@a,@b,@status,now()) on conflict(id) do update set status=@status,completed_at=case when @status='Completed' then now() end,updated_at=now() where team_training_enrollments.tenant_id=@tenant and team_training_enrollments.branch_id=@branch","Team360.TrainingEnrollment","team_training_enrollments",status=="Completed"?a:null,null,c=>{TeamDataService.Add(c,"a",a);TeamDataService.Add(c,"b",b);TeamDataService.Add(c,"status",status);},ct);return new(id,status);}
}
