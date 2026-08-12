namespace BarberSync.Application.Operations;

public sealed record OpenServiceOrderRequest(Guid ClientId, Guid? AppointmentId, string? Notes);
public sealed record AddServiceItemRequest(Guid ServiceId, Guid ProfessionalId, decimal Quantity = 1);
public sealed record AddProductItemRequest(Guid ProductId, decimal Quantity = 1, Guid? ProfessionalId = null);
public sealed record UpdateOrderItemRequest(decimal Quantity, decimal Discount = 0, Guid? ProfessionalId = null);
public sealed record ApplyDiscountRequest(decimal Amount, string Reason);
public sealed record ApplyCouponRequest(string Code);
public sealed record ApplyCashbackRequest(decimal Amount);
public sealed record PaymentSplitRequest(string Method, decimal Amount, decimal? ReceivedAmount = null);
public sealed record RegisterPaymentRequest(string IdempotencyKey, IReadOnlyList<PaymentSplitRequest> Splits);
public sealed record RefundPaymentRequest(string Reason);
public sealed record ServiceOrderItemResponse(Guid Id, string Type, Guid? ServiceId, Guid? ProductId, Guid? ProfessionalId, string Description, decimal Quantity, decimal UnitPrice, decimal Discount, decimal Total);
public sealed record ServiceOrderResponse(Guid Id, string Number, Guid ClientId, Guid? AppointmentId, string Status, decimal Subtotal, decimal Discount, decimal Surcharge, decimal Total, decimal Paid, decimal Balance, IReadOnlyList<ServiceOrderItemResponse> Items);
public sealed record PaymentResponse(Guid Id, Guid ServiceOrderId, string Status, decimal Amount, decimal Change, IReadOnlyList<PaymentSplitRequest> Splits, bool Replayed = false, string? OrderStatus = null, decimal? OrderBalance = null);

public static class PaymentRules
{
    public static decimal ValidateAndTotal(IReadOnlyList<PaymentSplitRequest> splits, decimal balance)
    {
        if (splits.Count == 0)
            throw new InvalidOperationException("Informe ao menos uma forma de pagamento.");

        if (splits.Any(split => split.Amount <= 0))
            throw new InvalidOperationException("Os valores de pagamento devem ser positivos.");

        foreach (var split in splits)
        {
            var isCash = split.Method.Equals("Cash", StringComparison.OrdinalIgnoreCase);
            if (!isCash && split.ReceivedAmount is not null)
                throw new InvalidOperationException("Valor recebido e troco são permitidos somente para dinheiro.");
            if (isCash && split.ReceivedAmount is { } received && received < split.Amount)
                throw new InvalidOperationException("O valor recebido em dinheiro não pode ser menor que o valor aplicado.");
        }

        var amount = splits.Sum(split => split.Amount);
        if (amount > balance)
            throw new InvalidOperationException("A soma dos pagamentos não pode superar o saldo da comanda.");

        return amount;
    }

    public static string OrderStatus(decimal amount, decimal balance) =>
        amount == balance ? "Paid" : "PartiallyPaid";
}

public interface IServiceOrderRepository
{
    Task<IReadOnlyList<ServiceOrderResponse>> ListAsync(Guid tenant, Guid branch, CancellationToken ct);
    Task<ServiceOrderResponse?> GetAsync(Guid tenant, Guid branch, Guid id, CancellationToken ct);
    Task<ServiceOrderResponse> OpenAsync(Guid tenant, Guid branch, OpenServiceOrderRequest request, CancellationToken ct);
    Task<ServiceOrderResponse> AddServiceAsync(Guid tenant, Guid branch, Guid id, AddServiceItemRequest request, CancellationToken ct);
    Task<ServiceOrderResponse> AddProductAsync(Guid tenant, Guid branch, Guid id, AddProductItemRequest request, CancellationToken ct);
    Task<ServiceOrderResponse> UpdateItemAsync(Guid tenant, Guid branch, Guid id, Guid itemId, UpdateOrderItemRequest request, CancellationToken ct);
    Task<ServiceOrderResponse> RemoveItemAsync(Guid tenant, Guid branch, Guid id, Guid itemId, CancellationToken ct);
    Task<ServiceOrderResponse> ApplyDiscountAsync(Guid tenant, Guid branch, Guid user, Guid id, ApplyDiscountRequest request, CancellationToken ct);
    Task<ServiceOrderResponse> ApplyCouponAsync(Guid tenant, Guid branch, Guid user, Guid id, ApplyCouponRequest request, CancellationToken ct);
    Task<ServiceOrderResponse> ApplyCashbackAsync(Guid tenant, Guid branch, Guid user, Guid id, ApplyCashbackRequest request, CancellationToken ct);
}

public interface IPaymentRepository
{
    Task<PaymentResponse> RegisterAsync(Guid tenant, Guid branch, Guid user, Guid orderId, RegisterPaymentRequest request, CancellationToken ct);
    Task<PaymentResponse> RefundAsync(Guid tenant, Guid branch, Guid user, Guid paymentId, RefundPaymentRequest request, CancellationToken ct);
}
public interface IStockService { }
public interface ICommissionService { }
public interface ILoyaltyService { }

public interface IServiceOrderService
{
    Task<IReadOnlyList<ServiceOrderResponse>> ListAsync(CancellationToken ct);
    Task<ServiceOrderResponse?> GetAsync(Guid id, CancellationToken ct);
    Task<ServiceOrderResponse> OpenAsync(OpenServiceOrderRequest request, CancellationToken ct);
    Task<ServiceOrderResponse> AddServiceAsync(Guid id, AddServiceItemRequest request, CancellationToken ct);
    Task<ServiceOrderResponse> AddProductAsync(Guid id, AddProductItemRequest request, CancellationToken ct);
    Task<ServiceOrderResponse> UpdateItemAsync(Guid id, Guid itemId, UpdateOrderItemRequest request, CancellationToken ct);
    Task<ServiceOrderResponse> RemoveItemAsync(Guid id, Guid itemId, CancellationToken ct);
    Task<ServiceOrderResponse> ApplyDiscountAsync(Guid id, ApplyDiscountRequest request, CancellationToken ct);
    Task<ServiceOrderResponse> ApplyCouponAsync(Guid id, ApplyCouponRequest request, CancellationToken ct);
    Task<ServiceOrderResponse> ApplyCashbackAsync(Guid id, ApplyCashbackRequest request, CancellationToken ct);
}
public interface IPaymentService
{
    Task<PaymentResponse> RegisterAsync(Guid orderId, RegisterPaymentRequest request, CancellationToken ct);
    Task<PaymentResponse> RefundAsync(Guid paymentId, RefundPaymentRequest request, CancellationToken ct);
}

public sealed class ServiceOrderService(IServiceOrderRepository repository, Abstractions.ICurrentUserContext current) : IServiceOrderService
{
    public Task<IReadOnlyList<ServiceOrderResponse>> ListAsync(CancellationToken ct) => repository.ListAsync(current.TenantId,current.BranchId,ct);
    public Task<ServiceOrderResponse?> GetAsync(Guid id,CancellationToken ct) => repository.GetAsync(current.TenantId,current.BranchId,id,ct);
    public Task<ServiceOrderResponse> OpenAsync(OpenServiceOrderRequest request,CancellationToken ct) => repository.OpenAsync(current.TenantId,current.BranchId,request,ct);
    public Task<ServiceOrderResponse> AddServiceAsync(Guid id,AddServiceItemRequest request,CancellationToken ct) => repository.AddServiceAsync(current.TenantId,current.BranchId,id,request,ct);
    public Task<ServiceOrderResponse> AddProductAsync(Guid id,AddProductItemRequest request,CancellationToken ct) => repository.AddProductAsync(current.TenantId,current.BranchId,id,request,ct);
    public Task<ServiceOrderResponse> UpdateItemAsync(Guid id,Guid itemId,UpdateOrderItemRequest request,CancellationToken ct) => repository.UpdateItemAsync(current.TenantId,current.BranchId,id,itemId,request,ct);
    public Task<ServiceOrderResponse> RemoveItemAsync(Guid id,Guid itemId,CancellationToken ct) => repository.RemoveItemAsync(current.TenantId,current.BranchId,id,itemId,ct);
    public Task<ServiceOrderResponse> ApplyDiscountAsync(Guid id,ApplyDiscountRequest request,CancellationToken ct) => repository.ApplyDiscountAsync(current.TenantId,current.BranchId,current.UserId,id,request,ct);
    public Task<ServiceOrderResponse> ApplyCouponAsync(Guid id,ApplyCouponRequest request,CancellationToken ct) => repository.ApplyCouponAsync(current.TenantId,current.BranchId,current.UserId,id,request,ct);
    public Task<ServiceOrderResponse> ApplyCashbackAsync(Guid id,ApplyCashbackRequest request,CancellationToken ct) => repository.ApplyCashbackAsync(current.TenantId,current.BranchId,current.UserId,id,request,ct);
}
public sealed class PaymentService(IPaymentRepository repository, Abstractions.ICurrentUserContext current) : IPaymentService
{
    public Task<PaymentResponse> RegisterAsync(Guid orderId,RegisterPaymentRequest request,CancellationToken ct) => repository.RegisterAsync(current.TenantId,current.BranchId,current.UserId,orderId,request,ct);
    public Task<PaymentResponse> RefundAsync(Guid paymentId,RefundPaymentRequest request,CancellationToken ct) => repository.RefundAsync(current.TenantId,current.BranchId,current.UserId,paymentId,request,ct);
}
