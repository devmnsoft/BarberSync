namespace BarberSync.Application.Operations;

public sealed record OpenCashRegisterRequest(decimal OpeningBalance, string? Note = null);
public sealed record CashMovementRequest(decimal Amount, string Reason, string? Category = null);
public sealed record CloseCashRegisterRequest(decimal ActualBalance, string? Note = null);
public sealed record CashMovementResponse(Guid Id, string Type, decimal Amount, string Description, DateTimeOffset CreatedAt);
public sealed record CashRegisterResponse(Guid Id, string Status, decimal OpeningBalance, decimal Inflows, decimal Outflows,
    decimal ExpectedBalance, decimal? ActualBalance, decimal Difference, DateTimeOffset OpenedAt,
    DateTimeOffset? ClosedAt, IReadOnlyList<CashMovementResponse> Movements);

public static class CashRegisterRules
{
    public static void ValidateOpening(decimal balance)
    {
        if (balance < 0) throw new InvalidOperationException("O saldo inicial não pode ser negativo.");
    }

    public static void ValidateMovement(CashMovementRequest request, bool categoryRequired = false)
    {
        if (request.Amount <= 0) throw new InvalidOperationException("O valor da movimentação deve ser maior que zero.");
        if (string.IsNullOrWhiteSpace(request.Reason)) throw new InvalidOperationException("Informe o motivo da movimentação.");
        if (categoryRequired && string.IsNullOrWhiteSpace(request.Category)) throw new InvalidOperationException("Informe a categoria da despesa.");
    }

    public static void ValidateClosing(decimal expected, CloseCashRegisterRequest request)
    {
        if (request.ActualBalance < 0) throw new InvalidOperationException("O valor conferido não pode ser negativo.");
        if (request.ActualBalance != expected && string.IsNullOrWhiteSpace(request.Note))
            throw new InvalidOperationException("Informe uma observação para justificar a divergência.");
    }
}

public interface ICashRegisterRepository
{
    Task<CashRegisterResponse?> CurrentAsync(Guid tenant, Guid branch, CancellationToken ct);
    Task<IReadOnlyList<CashRegisterResponse>> HistoryAsync(Guid tenant, Guid branch, CancellationToken ct);
    Task<CashRegisterResponse> OpenAsync(Guid tenant, Guid branch, Guid user, OpenCashRegisterRequest request, CancellationToken ct);
    Task<CashRegisterResponse> MoveAsync(Guid tenant, Guid branch, Guid user, Guid id, string type, CashMovementRequest request, CancellationToken ct);
    Task<CashRegisterResponse> CloseAsync(Guid tenant, Guid branch, Guid user, Guid id, CloseCashRegisterRequest request, CancellationToken ct);
}

public interface ICashRegisterService
{
    Task<CashRegisterResponse?> CurrentAsync(CancellationToken ct);
    Task<IReadOnlyList<CashRegisterResponse>> HistoryAsync(CancellationToken ct);
    Task<CashRegisterResponse> OpenAsync(OpenCashRegisterRequest request, CancellationToken ct);
    Task<CashRegisterResponse> SupplyAsync(Guid id, CashMovementRequest request, CancellationToken ct);
    Task<CashRegisterResponse> WithdrawalAsync(Guid id, CashMovementRequest request, CancellationToken ct);
    Task<CashRegisterResponse> ExpenseAsync(Guid id, CashMovementRequest request, CancellationToken ct);
    Task<CashRegisterResponse> CloseAsync(Guid id, CloseCashRegisterRequest request, CancellationToken ct);
}

public sealed class CashRegisterService(ICashRegisterRepository repository, Abstractions.ICurrentUserContext current) : ICashRegisterService
{
    public Task<CashRegisterResponse?> CurrentAsync(CancellationToken ct) => repository.CurrentAsync(current.TenantId, current.BranchId, ct);
    public Task<IReadOnlyList<CashRegisterResponse>> HistoryAsync(CancellationToken ct) => repository.HistoryAsync(current.TenantId, current.BranchId, ct);
    public Task<CashRegisterResponse> OpenAsync(OpenCashRegisterRequest request, CancellationToken ct) => repository.OpenAsync(current.TenantId, current.BranchId, current.UserId, request, ct);
    public Task<CashRegisterResponse> SupplyAsync(Guid id, CashMovementRequest request, CancellationToken ct) => repository.MoveAsync(current.TenantId, current.BranchId, current.UserId, id, "Supply", request, ct);
    public Task<CashRegisterResponse> WithdrawalAsync(Guid id, CashMovementRequest request, CancellationToken ct) => repository.MoveAsync(current.TenantId, current.BranchId, current.UserId, id, "Withdrawal", request, ct);
    public Task<CashRegisterResponse> ExpenseAsync(Guid id, CashMovementRequest request, CancellationToken ct) => repository.MoveAsync(current.TenantId, current.BranchId, current.UserId, id, "Expense", request, ct);
    public Task<CashRegisterResponse> CloseAsync(Guid id, CloseCashRegisterRequest request, CancellationToken ct) => repository.CloseAsync(current.TenantId, current.BranchId, current.UserId, id, request, ct);
}
