using System.Data.Common;
using BarberSync.Application.Abstractions;
using BarberSync.Application.Operations;

namespace BarberSync.Infrastructure.Repositories;

public sealed class PostgresCashRegisterRepository(IDbConnectionFactory connections) : ICashRegisterRepository
{
    public async Task<CashRegisterResponse?> CurrentAsync(Guid tenant, Guid branch, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        var id = await FindId(connection, tenant, branch, true, ct);
        return id is null ? null : await Read(connection, tenant, branch, id.Value, ct);
    }

    public async Task<IReadOnlyList<CashRegisterResponse>> HistoryAsync(Guid tenant, Guid branch, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        var ids = new List<Guid>();
        await using (var command = Command(connection, "SELECT id FROM barber.cash_registers WHERE tenant_id=@tenant AND branch_id=@branch AND deleted_at IS NULL ORDER BY opened_at DESC LIMIT 100"))
        {
            Add(command, "tenant", tenant); Add(command, "branch", branch);
            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct)) ids.Add(reader.GetGuid(0));
        }
        var result = new List<CashRegisterResponse>();
        foreach (var id in ids) result.Add((await Read(connection, tenant, branch, id, ct))!);
        return result;
    }

    public async Task<CashRegisterResponse> OpenAsync(Guid tenant, Guid branch, Guid user, OpenCashRegisterRequest request, CancellationToken ct)
    {
        CashRegisterRules.ValidateOpening(request.OpeningBalance);
        await using var connection = await connections.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        if (await FindId(connection, tenant, branch, true, ct, transaction) is not null)
            throw new InvalidOperationException("Já existe um caixa aberto para esta unidade.");
        var id = Guid.NewGuid();
        await using (var command = Command(connection, "INSERT INTO barber.cash_registers(id,tenant_id,branch_id,opened_by,opening_balance,status,payload) VALUES(@id,@tenant,@branch,@user,@balance,'Open',jsonb_build_object('openingNote',@note))", transaction))
        {
            Add(command, "id", id); Add(command, "tenant", tenant); Add(command, "branch", branch); Add(command, "user", user);
            Add(command, "balance", request.OpeningBalance); Add(command, "note", request.Note);
            await command.ExecuteNonQueryAsync(ct);
        }
        await Audit(connection, transaction, tenant, branch, user, "Cash.Open", id, request.Note ?? "Abertura de caixa", ct);
        await transaction.CommitAsync(ct);
        return (await Read(connection, tenant, branch, id, ct))!;
    }

    public async Task<CashRegisterResponse> MoveAsync(Guid tenant, Guid branch, Guid user, Guid id, string type, CashMovementRequest request, CancellationToken ct)
    {
        CashRegisterRules.ValidateMovement(request, type == "Expense");
        await using var connection = await connections.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        await EnsureOpen(connection, transaction, tenant, branch, id, ct);
        var movementId = Guid.NewGuid();
        var signedAmount = type is "Withdrawal" or "Expense" ? -request.Amount : request.Amount;
        var description = type == "Expense" ? $"{request.Category}: {request.Reason.Trim()}" : request.Reason.Trim();
        await using (var command = Command(connection, "INSERT INTO barber.cash_movements(id,tenant_id,branch_id,cash_register_id,type,amount,description,origin,created_by) VALUES(@id,@tenant,@branch,@register,@type,@amount,@description,'Manual',@user)", transaction))
        {
            Add(command, "id", movementId); Add(command, "tenant", tenant); Add(command, "branch", branch); Add(command, "register", id);
            Add(command, "type", type); Add(command, "amount", signedAmount); Add(command, "description", description);
            await command.ExecuteNonQueryAsync(ct);
        }
        await Audit(connection, transaction, tenant, branch, user, $"Cash.{type}", id, description, ct);
        await transaction.CommitAsync(ct);
        return (await Read(connection, tenant, branch, id, ct))!;
    }

    public async Task<CashRegisterResponse> CloseAsync(Guid tenant, Guid branch, Guid user, Guid id, CloseCashRegisterRequest request, CancellationToken ct)
    {
        await using var connection = await connections.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        await EnsureOpen(connection, transaction, tenant, branch, id, ct);
        var expected = await Expected(connection, transaction, id, ct);
        CashRegisterRules.ValidateClosing(expected, request);
        await using (var command = Command(connection, "UPDATE barber.cash_registers SET status='Closed',closed_by=@user,closed_at=now(),expected_balance=@expected,actual_balance=@actual,updated_at=now(),payload=payload||jsonb_build_object('closingNote',@note) WHERE id=@id", transaction))
        {
            Add(command, "user", user); Add(command, "expected", expected); Add(command, "actual", request.ActualBalance);
            Add(command, "note", request.Note); Add(command, "id", id); await command.ExecuteNonQueryAsync(ct);
        }
        await Audit(connection, transaction, tenant, branch, user, "Cash.Close", id, request.Note ?? "Fechamento conferido", ct);
        await transaction.CommitAsync(ct);
        return (await Read(connection, tenant, branch, id, ct))!;
    }

    private static async Task<CashRegisterResponse?> Read(DbConnection connection, Guid tenant, Guid branch, Guid id, CancellationToken ct)
    {
        string status; decimal opening, inflows, outflows, expected; decimal? actual; DateTimeOffset opened; DateTimeOffset? closed;
        const string sql = """
            SELECT r.status,r.opening_balance,
              COALESCE(sum(t.amount) FILTER(WHERE t.amount>0),0),COALESCE(-sum(t.amount) FILTER(WHERE t.amount<0),0),
              r.opening_balance+COALESCE(sum(t.amount),0),r.actual_balance,r.opened_at,r.closed_at
            FROM barber.cash_registers r LEFT JOIN barber.cash_movements t ON t.cash_register_id=r.id
            WHERE r.id=@id AND r.tenant_id=@tenant AND r.branch_id=@branch AND r.deleted_at IS NULL
            GROUP BY r.id
            """;
        await using (var command = Command(connection, sql))
        {
            Add(command, "id", id); Add(command, "tenant", tenant); Add(command, "branch", branch);
            await using var reader = await command.ExecuteReaderAsync(ct); if (!await reader.ReadAsync(ct)) return null;
            status=reader.GetString(0); opening=reader.GetDecimal(1); inflows=reader.GetDecimal(2); outflows=reader.GetDecimal(3);
            expected=reader.GetDecimal(4); actual=reader.IsDBNull(5)?null:reader.GetDecimal(5); opened=reader.GetFieldValue<DateTimeOffset>(6); closed=reader.IsDBNull(7)?null:reader.GetFieldValue<DateTimeOffset>(7);
        }
        var movements = new List<CashMovementResponse>();
        await using (var command = Command(connection, "SELECT id,type,amount,COALESCE(description,''),created_at,payment_id FROM barber.cash_movements WHERE cash_register_id=@id AND tenant_id=@tenant AND branch_id=@branch ORDER BY created_at DESC"))
        {
            Add(command, "id", id); Add(command, "tenant", tenant); Add(command, "branch", branch);
            await using var reader = await command.ExecuteReaderAsync(ct);
            while(await reader.ReadAsync(ct)) movements.Add(new(reader.GetGuid(0),reader.GetString(1),reader.GetDecimal(2),reader.GetString(3),reader.GetFieldValue<DateTimeOffset>(4),reader.IsDBNull(5)?null:reader.GetGuid(5)));
        }
        return new(id,status,opening,inflows,outflows,expected,actual,actual is null?0:actual.Value-expected,opened,closed,movements);
    }

    private static async Task<Guid?> FindId(DbConnection connection, Guid tenant, Guid branch, bool openOnly, CancellationToken ct, DbTransaction? transaction = null)
    {
        await using var command = Command(connection, $"SELECT id FROM barber.cash_registers WHERE tenant_id=@tenant AND branch_id=@branch AND deleted_at IS NULL{(openOnly ? " AND status='Open'" : "")} ORDER BY opened_at DESC LIMIT 1", transaction);
        Add(command, "tenant", tenant); Add(command, "branch", branch); var value = await command.ExecuteScalarAsync(ct); return value is Guid id ? id : null;
    }
    private static async Task EnsureOpen(DbConnection connection, DbTransaction transaction, Guid tenant, Guid branch, Guid id, CancellationToken ct)
    { await using var command=Command(connection,"SELECT 1 FROM barber.cash_registers WHERE id=@id AND tenant_id=@tenant AND branch_id=@branch AND status='Open' AND deleted_at IS NULL FOR UPDATE",transaction);Add(command,"id",id);Add(command,"tenant",tenant);Add(command,"branch",branch);if(await command.ExecuteScalarAsync(ct) is null)throw new InvalidOperationException("O caixa não está aberto."); }
    private static async Task<decimal> Expected(DbConnection connection, DbTransaction transaction, Guid id, CancellationToken ct)
    { await using var command=Command(connection,"SELECT r.opening_balance+COALESCE(sum(t.amount),0) FROM barber.cash_registers r LEFT JOIN barber.cash_movements t ON t.cash_register_id=r.id WHERE r.id=@id GROUP BY r.id",transaction);Add(command,"id",id);return Convert.ToDecimal(await command.ExecuteScalarAsync(ct)); }
    private static async Task Audit(DbConnection connection, DbTransaction transaction, Guid tenant, Guid branch, Guid user, string operation, Guid entity, string description, CancellationToken ct)
    { await using var command=Command(connection,"INSERT INTO barber.audit_logs(id,tenant_id,branch_id,user_id,operation,entity_name,entity_id,description,module,action) VALUES(@id,@tenant,@branch,@user,@operation,'CashRegister',@entity,@description,'Cash',@operation)",transaction);Add(command,"id",Guid.NewGuid());Add(command,"tenant",tenant);Add(command,"branch",branch);Add(command,"user",user);Add(command,"operation",operation);Add(command,"entity",entity);Add(command,"description",description);await command.ExecuteNonQueryAsync(ct); }
    private static DbCommand Command(DbConnection connection,string sql,DbTransaction? transaction=null){var command=connection.CreateCommand();command.CommandText=sql;command.Transaction=transaction;return command;}
    private static void Add(DbCommand command,string name,object? value){var parameter=command.CreateParameter();parameter.ParameterName=name;parameter.Value=value??DBNull.Value;command.Parameters.Add(parameter);}
}
