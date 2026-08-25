using System.Data.Common;
using BarberSync.Application.Abstractions;
using BarberSync.Application.Operations;

namespace BarberSync.Infrastructure.Repositories;

public sealed class PostgresAppointmentRepository(IDbConnectionFactory connections) : IAppointmentRepository
{
    private const string AppointmentProjectionSql = """
        SELECT a.id,a.client_id,COALESCE(c.name, ''),a.professional_id,COALESCE(p.name, ''),a.service_id,COALESCE(s.name, ''),
               a.scheduled_start,a.scheduled_end,COALESCE(s.duration_minutes, 30),a.status,a.origin,a.notes,a.cancellation_reason
          FROM barber.appointments a
          LEFT JOIN barber.clients c ON c.id=a.client_id
          LEFT JOIN barber.professionals p ON p.id=a.professional_id
          LEFT JOIN barber.services s ON s.id=a.service_id
        """;

    private const string CreateAppointmentSql = """
        INSERT INTO barber.appointments
            (id,tenant_id,branch_id,client_id,professional_id,service_id,scheduled_start,scheduled_end,status,origin,notes)
        VALUES (@id,@tenant,@branch,@client,@professional,@service,@start,@finish,@status,@origin,@notes)
        """;

    private const string UpdateAppointmentSql = """
        UPDATE barber.appointments
        SET client_id=@client,professional_id=@professional,service_id=@service,
            scheduled_start=@start,scheduled_end=@finish,origin=@origin,notes=@notes,updated_at=now()
        WHERE id=@id AND tenant_id=@tenant AND branch_id=@branch AND deleted_at IS NULL
        """;

    private const string ChangeStatusSql = """
        UPDATE barber.appointments
        SET status=@status,cancellation_reason=CASE WHEN @status='Cancelled' THEN @reason ELSE cancellation_reason END,
            checked_in_at=CASE WHEN @status='CheckedIn' THEN now() ELSE checked_in_at END,
            started_at=CASE WHEN @status='InService' THEN now() ELSE started_at END,
            completed_at=CASE WHEN @status='Finished' THEN now() ELSE completed_at END,updated_at=now()
        WHERE id=@id AND tenant_id=@tenant AND branch_id=@branch AND status=@expected AND deleted_at IS NULL
        """;

    public async Task<IReadOnlyList<AppointmentResponse>> ListAsync(Guid tenantId, Guid branchId, AppointmentFilter filter, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = AppointmentProjectionSql + """
             WHERE a.tenant_id=@tenant AND a.branch_id=@branch AND a.deleted_at IS NULL
               AND (@from IS NULL OR a.scheduled_end >= @from) AND (@to IS NULL OR a.scheduled_start < @to)
               AND (@professional IS NULL OR a.professional_id=@professional) AND (@service IS NULL OR a.service_id=@service)
               AND (@status IS NULL OR a.status=@status) AND (@origin IS NULL OR a.origin=@origin)
             ORDER BY a.scheduled_start
            """;
        Add(command, "tenant", tenantId); Add(command, "branch", branchId); Add(command, "from", filter.From); Add(command, "to", filter.To);
        Add(command, "professional", filter.ProfessionalId); Add(command, "service", filter.ServiceId); Add(command, "status", filter.Status); Add(command, "origin", filter.Origin);
        var rows = new List<AppointmentResponse>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct)) rows.Add(Read(reader));
        return rows;
    }

    public async Task<AppointmentResponse?> GetAsync(Guid tenantId, Guid branchId, Guid id, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = AppointmentProjectionSql + " WHERE a.id=@id AND a.tenant_id=@tenant AND a.branch_id=@branch AND a.deleted_at IS NULL";
        Add(command, "id", id); Add(command, "tenant", tenantId); Add(command, "branch", branchId);
        await using var reader = await command.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Read(reader) : null;
    }

    public async Task<int?> GetServiceDurationAsync(Guid tenantId, Guid branchId, Guid serviceId, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT duration_minutes FROM barber.services WHERE id=@id AND tenant_id=@tenant AND (branch_id IS NULL OR branch_id=@branch) AND status='Active' AND deleted_at IS NULL";
        Add(command, "id", serviceId); Add(command, "tenant", tenantId); Add(command, "branch", branchId);
        var value = await command.ExecuteScalarAsync(ct); return value is null ? null : Convert.ToInt32(value);
    }

    public async Task<bool> HasConflictAsync(Guid tenantId, Guid branchId, Guid professionalId, DateTimeOffset start, DateTimeOffset end, Guid? exceptId, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT EXISTS(SELECT 1 FROM barber.appointments
             WHERE tenant_id=@tenant AND branch_id=@branch AND professional_id=@professional AND deleted_at IS NULL
               AND status NOT IN ('Cancelled','NoShow') AND scheduled_start < @finish AND scheduled_end > @start
               AND (@except IS NULL OR id<>@except))
            """;
        Add(command, "tenant", tenantId); Add(command, "branch", branchId); Add(command, "professional", professionalId); Add(command, "start", start); Add(command, "finish", end); Add(command, "except", exceptId);
        return (bool)(await command.ExecuteScalarAsync(ct) ?? false);
    }

    public async Task<string?> GetUnavailabilityReasonAsync(Guid tenantId, Guid branchId, Guid professionalId, Guid serviceId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            WITH local_slot AS (
              SELECT (@start AT TIME ZONE 'America/Sao_Paulo') AS starts,
                     (@finish AT TIME ZONE 'America/Sao_Paulo') AS finishes
            )
            SELECT CASE
              WHEN NOT EXISTS (SELECT 1 FROM barber.professionals p WHERE p.id=@professional AND p.tenant_id=@tenant AND p.branch_id=@branch AND p.status='Active' AND p.is_active AND p.deleted_at IS NULL)
                THEN 'Profissional inativo ou indisponível nesta unidade.'
              WHEN NOT EXISTS (SELECT 1 FROM barber.professional_services ps WHERE ps.professional_id=@professional AND ps.service_id=@service)
                THEN 'O profissional não executa o serviço selecionado.'
              WHEN NOT EXISTS (
                SELECT 1 FROM barber.professional_working_hours h, local_slot l
                WHERE h.tenant_id=@tenant AND h.branch_id=@branch AND h.professional_id=@professional AND h.is_active
                  AND h.day_of_week=EXTRACT(ISODOW FROM l.starts)::smallint
                  AND l.starts::date>=h.effective_from AND (h.effective_to IS NULL OR l.starts::date<=h.effective_to)
                  AND l.starts::time>=h.start_time AND l.finishes::time<=h.end_time
                  AND (h.break_start IS NULL OR NOT (l.starts::time<h.break_end AND l.finishes::time>h.break_start)))
                THEN 'Horário fora da escala ou durante a pausa do profissional.'
              WHEN EXISTS (
                SELECT 1 FROM barber.professional_schedule_blocks b
                WHERE b.tenant_id=@tenant AND b.branch_id=@branch AND b.professional_id=@professional
                  AND b.start_at<@finish AND b.end_at>@start)
                THEN 'Horário bloqueado na agenda do profissional.'
              WHEN EXISTS (
                SELECT 1 FROM barber.professional_time_off t
                WHERE t.tenant_id=@tenant AND t.branch_id=@branch AND t.professional_id=@professional AND t.status='Approved'
                  AND t.starts_at<@finish AND t.ends_at>@start)
                THEN 'Profissional em folga, férias ou indisponibilidade aprovada.'
              ELSE NULL END
            """;
        Add(command,"tenant",tenantId); Add(command,"branch",branchId); Add(command,"professional",professionalId);
        Add(command,"service",serviceId); Add(command,"start",start); Add(command,"finish",end);
        var result = await command.ExecuteScalarAsync(ct);
        return result is null or DBNull ? null : Convert.ToString(result);
    }

    public async Task<AppointmentResponse> CreateAsync(AppointmentDraft a, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = CreateAppointmentSql;
        Bind(command, a); await command.ExecuteNonQueryAsync(ct);
        return (await GetAsync(a.TenantId, a.BranchId, a.Id, ct))!;
    }

    public async Task<AppointmentResponse> UpdateAsync(AppointmentDraft a, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = UpdateAppointmentSql;
        Bind(command, a); if (await command.ExecuteNonQueryAsync(ct) != 1) throw new KeyNotFoundException("Agendamento não encontrado.");
        return (await GetAsync(a.TenantId, a.BranchId, a.Id, ct))!;
    }

    public async Task<AppointmentResponse> ChangeStatusAsync(Guid tenantId, Guid branchId, Guid id, string expectedStatus, string status, string? reason, Guid userId, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct); await using var tx = await connection.BeginTransactionAsync(ct);
        await using var command = connection.CreateCommand(); command.Transaction = tx;
        command.CommandText = ChangeStatusSql;
        Add(command,"status",status); Add(command,"reason",reason); Add(command,"id",id); Add(command,"tenant",tenantId); Add(command,"branch",branchId); Add(command,"expected",expectedStatus);
        if (await command.ExecuteNonQueryAsync(ct) != 1) throw new InvalidOperationException("O agendamento foi alterado por outra operação.");
        await History(connection, tx, tenantId, branchId, id, expectedStatus, status, null, null, reason, userId, ct); await tx.CommitAsync(ct);
        return (await GetAsync(tenantId, branchId, id, ct))!;
    }

    public async Task<AppointmentResponse> RescheduleAsync(Guid tenantId, Guid branchId, Guid id, DateTimeOffset start, DateTimeOffset end, string reason, Guid userId, CancellationToken ct)
    {
        var before = await GetAsync(tenantId, branchId, id, ct) ?? throw new KeyNotFoundException("Agendamento não encontrado.");
        await using var connection = await connections.OpenConnectionAsync(ct); await using var tx = await connection.BeginTransactionAsync(ct);
        await using var command = connection.CreateCommand(); command.Transaction = tx;
        command.CommandText = "UPDATE barber.appointments SET scheduled_start=@start,scheduled_end=@finish,updated_at=now() WHERE id=@id AND tenant_id=@tenant AND branch_id=@branch AND deleted_at IS NULL";
        Add(command,"start",start); Add(command,"finish",end); Add(command,"id",id); Add(command,"tenant",tenantId); Add(command,"branch",branchId); await command.ExecuteNonQueryAsync(ct);
        await History(connection, tx, tenantId, branchId, id, before.Status, before.Status, before.ScheduledStart, start, reason, userId, ct); await tx.CommitAsync(ct);
        return (await GetAsync(tenantId, branchId, id, ct))!;
    }

    private static async Task History(DbConnection c, DbTransaction tx, Guid tenant, Guid branch, Guid appointment, string from, string to, DateTimeOffset? oldStart, DateTimeOffset? newStart, string? reason, Guid user, CancellationToken ct)
    { await using var cmd=c.CreateCommand(); cmd.Transaction=tx; cmd.CommandText="INSERT INTO barber.appointment_history(id,tenant_id,branch_id,appointment_id,from_status,to_status,old_start,new_start,reason,changed_by) VALUES(@id,@tenant,@branch,@appointment,@from,@to,@old,@new,@reason,@user)"; Add(cmd,"id",Guid.NewGuid());Add(cmd,"tenant",tenant);Add(cmd,"branch",branch);Add(cmd,"appointment",appointment);Add(cmd,"from",from);Add(cmd,"to",to);Add(cmd,"old",oldStart);Add(cmd,"new",newStart);Add(cmd,"reason",reason);Add(cmd,"user",user);await cmd.ExecuteNonQueryAsync(ct); }
    private static AppointmentResponse Read(DbDataReader r) => new(r.GetGuid(0),r.GetGuid(1),r.GetString(2),r.GetGuid(3),r.GetString(4),r.GetGuid(5),r.GetString(6),r.GetFieldValue<DateTimeOffset>(7),r.GetFieldValue<DateTimeOffset>(8),r.GetInt32(9),r.GetString(10),r.GetString(11),r.IsDBNull(12)?null:r.GetString(12),r.IsDBNull(13)?null:r.GetString(13));
    private static void Bind(DbCommand c, AppointmentDraft a) { Add(c,"id",a.Id);Add(c,"tenant",a.TenantId);Add(c,"branch",a.BranchId);Add(c,"client",a.ClientId);Add(c,"professional",a.ProfessionalId);Add(c,"service",a.ServiceId);Add(c,"start",a.Start);Add(c,"finish",a.End);Add(c,"status",a.Status);Add(c,"origin",a.Origin);Add(c,"notes",a.Notes); }
    private static void Add(DbCommand command,string name,object? value) { var p=command.CreateParameter();p.ParameterName=name;p.Value=value??DBNull.Value;command.Parameters.Add(p); }
}
