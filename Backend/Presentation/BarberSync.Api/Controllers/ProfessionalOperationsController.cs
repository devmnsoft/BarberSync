using System.Data.Common;
using BarberSync.Api.Security;
using BarberSync.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace BarberSync.Api.Controllers;

/// <summary>Operações de equipe que alteram disponibilidade real da agenda.</summary>
[ApiController, Authorize, Route("api/professionals/{professionalId:guid}")]
public sealed class ProfessionalOperationsController(IDbConnectionFactory connections, ICurrentUserContext currentUser) : ControllerBase
{
    [HttpGet("operations"), RequirePermission("Professional.Read")]
    public async Task<IActionResult> Operations(Guid professionalId, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        if (!await ProfessionalExists(connection, professionalId, ct)) return NotFound();
        var schedules = await Query(connection, "SELECT id,day_of_week,start_time,end_time,break_start,break_end FROM barber.professional_working_hours WHERE tenant_id=@tenant AND branch_id=@branch AND professional_id=@professional AND is_active ORDER BY day_of_week", professionalId, ct);
        var services = await Query(connection, "SELECT s.id,s.name,s.duration_minutes,s.price,ps.commission_percent FROM barber.professional_services ps JOIN barber.services s ON s.id=ps.service_id WHERE s.tenant_id=@tenant AND (s.branch_id IS NULL OR s.branch_id=@branch) AND ps.professional_id=@professional AND s.deleted_at IS NULL ORDER BY s.name", professionalId, ct);
        var blocks = await Query(connection, "SELECT id,start_at,end_at,reason,description FROM barber.professional_schedule_blocks WHERE tenant_id=@tenant AND branch_id=@branch AND professional_id=@professional AND end_at>=now() ORDER BY start_at", professionalId, ct);
        return Ok(new { schedules, services, blocks });
    }

    [HttpPut("schedule"), RequirePermission("Professional.Update")]
    public async Task<IActionResult> ReplaceSchedule(Guid professionalId, IReadOnlyList<ScheduleRequest> schedule, CancellationToken ct)
    {
        if (schedule.Count == 0 || schedule.GroupBy(x => x.DayOfWeek).Any(x => x.Count() > 1) || schedule.Any(x => x.DayOfWeek is < 1 or > 7 || x.End <= x.Start || ((x.BreakStart is null) != (x.BreakEnd is null)) || (x.BreakStart is not null && (x.BreakEnd <= x.BreakStart || x.BreakStart < x.Start || x.BreakEnd > x.End))))
            return ValidationProblem("A escala contém dias, períodos ou pausas inválidos.");
        await using var connection = await connections.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        if (!await ProfessionalExists(connection, professionalId, ct, transaction)) return NotFound();
        await Execute(connection, transaction, "UPDATE barber.professional_working_hours SET is_active=false,updated_at=now() WHERE tenant_id=@tenant AND branch_id=@branch AND professional_id=@professional AND is_active", professionalId, ct);
        foreach (var item in schedule)
        {
            await using var command = Command(connection, transaction, "INSERT INTO barber.professional_working_hours(id,tenant_id,branch_id,professional_id,day_of_week,start_time,end_time,break_start,break_end) VALUES(@id,@tenant,@branch,@professional,@day,@start,@end,@breakStart,@breakEnd)", professionalId);
            Add(command,"id",Guid.NewGuid()); Add(command,"day",item.DayOfWeek); Add(command,"start",item.Start); Add(command,"end",item.End); Add(command,"breakStart",item.BreakStart); Add(command,"breakEnd",item.BreakEnd);
            await command.ExecuteNonQueryAsync(ct);
        }
        await Audit(connection, transaction, "Professional.ScheduleChanged", professionalId, ct);
        await transaction.CommitAsync(ct);
        return NoContent();
    }

    [HttpPut("services"), RequirePermission("Professional.Update")]
    public async Task<IActionResult> ReplaceServices(Guid professionalId, ServiceLinksRequest request, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        if (!await ProfessionalExists(connection, professionalId, ct, transaction)) return NotFound();
        await Execute(connection, transaction, "DELETE FROM barber.professional_services WHERE professional_id=@professional", professionalId, ct);
        foreach (var serviceId in request.ServiceIds.Distinct())
        {
            await using var command = Command(connection, transaction, "INSERT INTO barber.professional_services(professional_id,service_id) SELECT @professional,id FROM barber.services WHERE id=@service AND tenant_id=@tenant AND (branch_id IS NULL OR branch_id=@branch) AND status='Active' AND deleted_at IS NULL", professionalId);
            Add(command,"service",serviceId);
            if (await command.ExecuteNonQueryAsync(ct) != 1) return ValidationProblem($"Serviço {serviceId} não está ativo nesta unidade.");
        }
        await Audit(connection, transaction, "Professional.ServicesChanged", professionalId, ct);
        await transaction.CommitAsync(ct);
        return NoContent();
    }

    [HttpPost("blocks"), RequirePermission("Appointment.Block")]
    public async Task<IActionResult> Block(Guid professionalId, BlockRequest request, CancellationToken ct)
    {
        if (request.End <= request.Start || string.IsNullOrWhiteSpace(request.Reason)) return ValidationProblem("Informe um período válido e o motivo do bloqueio.");
        await using var connection = await connections.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        if (!await ProfessionalExists(connection, professionalId, ct, transaction)) return NotFound();
        await using (var conflict = Command(connection, transaction, "SELECT EXISTS(SELECT 1 FROM barber.appointments WHERE tenant_id=@tenant AND branch_id=@branch AND professional_id=@professional AND status IN ('Scheduled','Confirmed','CheckedIn','InService') AND scheduled_start<@end AND scheduled_end>@start AND deleted_at IS NULL)", professionalId))
        { Add(conflict,"start",request.Start); Add(conflict,"end",request.End); if ((bool)(await conflict.ExecuteScalarAsync(ct) ?? false)) return Conflict(new { message="O bloqueio colide com um atendimento ativo." }); }
        var id = Guid.NewGuid();
        await using (var command = Command(connection, transaction, "INSERT INTO barber.professional_schedule_blocks(id,tenant_id,branch_id,professional_id,start_at,end_at,reason,description,created_by) VALUES(@id,@tenant,@branch,@professional,@start,@end,@reason,@description,@user)", professionalId))
        { Add(command,"id",id); Add(command,"start",request.Start); Add(command,"end",request.End); Add(command,"reason",request.Reason.Trim()); Add(command,"description",request.Description); Add(command,"user",currentUser.UserId); await command.ExecuteNonQueryAsync(ct); }
        await Audit(connection, transaction, "Professional.ScheduleBlocked", professionalId, ct);
        await transaction.CommitAsync(ct);
        return Created($"/api/professionals/{professionalId}/operations", new { id });
    }

    private async Task<bool> ProfessionalExists(DbConnection connection, Guid id, CancellationToken ct, DbTransaction? tx = null)
    { await using var command=Command(connection,tx,"SELECT EXISTS(SELECT 1 FROM barber.professionals WHERE id=@professional AND tenant_id=@tenant AND branch_id=@branch AND deleted_at IS NULL)",id); return (bool)(await command.ExecuteScalarAsync(ct) ?? false); }
    private async Task<IReadOnlyList<Dictionary<string,object?>>> Query(DbConnection connection,string sql,Guid professional,CancellationToken ct)
    { await using var command=Command(connection,null,sql,professional);await using var reader=await command.ExecuteReaderAsync(ct);var rows=new List<Dictionary<string,object?>>();while(await reader.ReadAsync(ct)){var row=new Dictionary<string,object?>();for(var i=0;i<reader.FieldCount;i++)row[reader.GetName(i)]=reader.IsDBNull(i)?null:reader.GetValue(i);rows.Add(row);}return rows; }
    private async Task Execute(DbConnection connection,DbTransaction tx,string sql,Guid professional,CancellationToken ct){await using var command=Command(connection,tx,sql,professional);await command.ExecuteNonQueryAsync(ct);}
    private async Task Audit(DbConnection connection,DbTransaction tx,string operation,Guid entity,CancellationToken ct){await using var command=Command(connection,tx,"INSERT INTO barber.audit_logs(id,tenant_id,branch_id,user_id,operation,entity_name,entity_id,module,action,description) VALUES(@id,@tenant,@branch,@user,@operation,'professionals',@professional,'Equipe',@operation,'Alteração operacional de profissional')",entity);Add(command,"id",Guid.NewGuid());Add(command,"user",currentUser.UserId);Add(command,"operation",operation);await command.ExecuteNonQueryAsync(ct);}
    private DbCommand Command(DbConnection connection,DbTransaction? tx,string sql,Guid professional){var command=connection.CreateCommand();command.Transaction=tx;command.CommandText=sql;Add(command,"tenant",currentUser.TenantId);Add(command,"branch",currentUser.BranchId);Add(command,"professional",professional);return command;}
    private static void Add(DbCommand command,string name,object? value){var parameter=command.CreateParameter();parameter.ParameterName=name;parameter.Value=value??DBNull.Value;command.Parameters.Add(parameter);}

    public sealed record ScheduleRequest(short DayOfWeek, TimeOnly Start, TimeOnly End, TimeOnly? BreakStart, TimeOnly? BreakEnd);
    public sealed record ServiceLinksRequest(IReadOnlyList<Guid> ServiceIds);
    public sealed record BlockRequest(DateTimeOffset Start, DateTimeOffset End, string Reason, string? Description);
}
