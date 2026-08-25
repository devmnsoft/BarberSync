using System.Data.Common;
using BarberSync.Application.Abstractions;
using Npgsql;

namespace BarberSync.Api.Services.Team;

/// <summary>Relational team ledger. Every statement is scoped to the authenticated tenant and branch.</summary>
public sealed class TeamDataService(IConfiguration configuration, ICurrentUserContext currentUser)
{
    public async Task<IReadOnlyList<Dictionary<string, object?>>> QueryAsync(string sql, Action<DbCommand>? bind, CancellationToken ct)
    {
        await using var connection = await Open(ct);
        await using var command = Command(connection, null, sql);
        bind?.Invoke(command);
        await using var reader = await command.ExecuteReaderAsync(ct);
        var rows = new List<Dictionary<string, object?>>();
        while (await reader.ReadAsync(ct))
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.FieldCount; i++) row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            rows.Add(row);
        }
        return rows;
    }

    public async Task<object?> ScalarAsync(string sql, Action<DbCommand>? bind, CancellationToken ct)
    {
        await using var connection = await Open(ct); await using var command = Command(connection, null, sql); bind?.Invoke(command); return await command.ExecuteScalarAsync(ct);
    }

    public async Task<Guid> WriteAsync(string sql, string action, string entity, Guid? entityId, string? reason, Action<DbCommand>? bind, CancellationToken ct)
    {
        await using var connection = await Open(ct); await using var tx = await connection.BeginTransactionAsync(ct); await using var command = Command(connection, tx, sql);
        var id = entityId ?? Guid.NewGuid(); Add(command, "id", id); bind?.Invoke(command); if (await command.ExecuteNonQueryAsync(ct) == 0) throw new InvalidOperationException("O registro relacionado não existe, não está ativo ou não pode ser alterado neste estado.");
        await Audit(connection, tx, action, entity, id, reason, ct); await tx.CommitAsync(ct); return id;
    }

    public async Task<Guid> CreateSettlement(Guid professionalId, DateOnly from, DateOnly to, decimal discount, string? reason, CancellationToken ct)
    {
        if (to < from || discount < 0 || (discount > 0 && string.IsNullOrWhiteSpace(reason))) throw new ArgumentException("Período, desconto ou motivo inválido.");
        await using var connection = await Open(ct); await using var tx = await connection.BeginTransactionAsync(ct);
        var settlementId = Guid.NewGuid();
        await using (var command = Command(connection, tx, @"insert into barber.commission_settlements(id,tenant_id,branch_id,professional_id,period_start,period_end,gross_amount,discount_amount,net_amount,adjustment_reason,created_by)
select @id,@tenant,@branch,@professional,@from,@to,coalesce(sum(c.amount),0),@discount,greatest(coalesce(sum(c.amount),0)-@discount,0),@reason,@user
from barber.commissions c where c.tenant_id=@tenant and c.branch_id=@branch and c.professional_id=@professional and c.status='Available' and c.created_at::date between @from and @to
having count(*)>0"))
        { Add(command,"id",settlementId); Add(command,"professional",professionalId); Add(command,"from",from); Add(command,"to",to); Add(command,"discount",discount); Add(command,"reason",reason); Add(command,"user",currentUser.UserId); if(await command.ExecuteNonQueryAsync(ct)!=1) throw new InvalidOperationException("Nenhuma comissão aberta para o período."); }
        await using (var command = Command(connection, tx, @"insert into barber.commission_settlement_items(id,tenant_id,branch_id,settlement_id,commission_id,payment_id,amount,description)
select gen_random_uuid(),tenant_id,branch_id,@id,id,payment_id,amount,'Comissão confirmada no PDV' from barber.commissions where tenant_id=@tenant and branch_id=@branch and professional_id=@professional and status='Available' and created_at::date between @from and @to for update skip locked"))
        { Add(command,"id",settlementId); Add(command,"professional",professionalId); Add(command,"from",from); Add(command,"to",to); await command.ExecuteNonQueryAsync(ct); }
        await using (var command = Command(connection, tx, "update barber.commissions c set status='Settled',updated_at=now() from barber.commission_settlement_items i where i.settlement_id=@id and i.commission_id=c.id")) { Add(command,"id",settlementId); await command.ExecuteNonQueryAsync(ct); }
        await Audit(connection,tx,"Commission.SettlementClosed","commission_settlements",settlementId,reason,ct); await tx.CommitAsync(ct); return settlementId;
    }

    public async Task MarkSettlement(Guid id, bool paid, string? method, string? reference, CancellationToken ct)
    {
        if (paid && (string.IsNullOrWhiteSpace(method) || string.IsNullOrWhiteSpace(reference))) throw new ArgumentException("Método e referência são obrigatórios.");
        await using var connection=await Open(ct); await using var tx=await connection.BeginTransactionAsync(ct);
        var sql=paid ? @"update barber.commission_settlements set status='Paid',paid_at=now(),updated_at=now() where id=@id and tenant_id=@tenant and branch_id=@branch and status='Approved' returning professional_id,net_amount" : @"update barber.commission_settlements set status='Approved',approved_by=@user,approved_at=now(),updated_at=now() where id=@id and tenant_id=@tenant and branch_id=@branch and status='Closed' returning professional_id,net_amount";
        Guid professional; decimal amount;
        await using(var command=Command(connection,tx,sql)){Add(command,"id",id);Add(command,"user",currentUser.UserId);await using var reader=await command.ExecuteReaderAsync(ct);if(!await reader.ReadAsync(ct))throw new InvalidOperationException("Settlement não está no estado exigido.");professional=reader.GetGuid(0);amount=reader.GetDecimal(1);}
        if(paid){await using var payout=Command(connection,tx,"insert into barber.professional_payouts(id,tenant_id,branch_id,professional_id,settlement_id,amount,payment_method,reference,paid_at,created_by) values(@payout,@tenant,@branch,@professional,@id,@amount,@method,@reference,now(),@user)");Add(payout,"payout",Guid.NewGuid());Add(payout,"professional",professional);Add(payout,"id",id);Add(payout,"amount",amount);Add(payout,"method",method);Add(payout,"reference",reference);Add(payout,"user",currentUser.UserId);await payout.ExecuteNonQueryAsync(ct);}
        await Audit(connection,tx,paid?"Commission.SettlementPaid":"Commission.SettlementApproved","commission_settlements",id,reference,ct);await tx.CommitAsync(ct);
    }

    public static void Add(DbCommand command,string name,object? value){var parameter=command.CreateParameter();parameter.ParameterName=name;parameter.Value=value??DBNull.Value;command.Parameters.Add(parameter);}
    private async Task<NpgsqlConnection> Open(CancellationToken ct){var cs=configuration.GetConnectionString("DefaultConnection")??throw new InvalidOperationException("ConnectionStrings:DefaultConnection não configurada.");var connection=new NpgsqlConnection(cs);await connection.OpenAsync(ct);return connection;}
    private DbCommand Command(DbConnection connection,DbTransaction? tx,string sql){var command=connection.CreateCommand();command.Transaction=tx;command.CommandText=sql;Add(command,"tenant",currentUser.TenantId);Add(command,"branch",currentUser.BranchId);return command;}
    private async Task Audit(DbConnection connection,DbTransaction tx,string action,string entity,Guid id,string? reason,CancellationToken ct){await using var command=Command(connection,tx,"insert into barber.audit_logs(id,tenant_id,branch_id,user_id,operation,entity_name,entity_id,module,action,description) values(@audit,@tenant,@branch,@user,@action,@entity,@id,@module,@action,@reason)");Add(command,"audit",Guid.NewGuid());Add(command,"user",currentUser.UserId);Add(command,"action",action);Add(command,"entity",entity);Add(command,"id",id);Add(command,"module",action.StartsWith("Finance.",StringComparison.Ordinal)?"Financeiro":"Equipe");Add(command,"reason",reason??"Alteração operacional auditada");await command.ExecuteNonQueryAsync(ct);}
}
