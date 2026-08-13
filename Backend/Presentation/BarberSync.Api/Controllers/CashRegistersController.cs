using BarberSync.Api.Security;
using BarberSync.Application.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Authorize, Route("api/cash-registers")]
public sealed class CashRegistersController(ICashRegisterService cash) : ControllerBase
{
    [HttpGet("current"), RequirePermission("Cash.View")]
    public Task<CashRegisterResponse?> Current(CancellationToken ct) => cash.CurrentAsync(ct);

    [HttpGet("history"), RequirePermission("Cash.View")]
    public Task<IReadOnlyList<CashRegisterResponse>> History(CancellationToken ct) => cash.HistoryAsync(ct);

    [HttpPost("open"), RequirePermission("Cash.Open")]
    public Task<CashRegisterResponse> Open(OpenCashRegisterRequest request, CancellationToken ct) => cash.OpenAsync(request, ct);

    [HttpPost("{id:guid}/supply"), RequirePermission("Cash.Supply")]
    public Task<CashRegisterResponse> Supply(Guid id, CashMovementRequest request, CancellationToken ct) => cash.SupplyAsync(id, request, ct);

    [HttpPost("{id:guid}/withdrawal"), RequirePermission("Cash.Withdraw")]
    public Task<CashRegisterResponse> Withdrawal(Guid id, CashMovementRequest request, CancellationToken ct) => cash.WithdrawalAsync(id, request, ct);

    [HttpPost("{id:guid}/expense"), RequirePermission("Cash.Withdraw")]
    public Task<CashRegisterResponse> Expense(Guid id, CashMovementRequest request, CancellationToken ct) => cash.ExpenseAsync(id, request, ct);

    [HttpPost("{id:guid}/close"), RequirePermission("Cash.Close")]
    public Task<CashRegisterResponse> Close(Guid id, CloseCashRegisterRequest request, CancellationToken ct) => cash.CloseAsync(id, request, ct);
}
