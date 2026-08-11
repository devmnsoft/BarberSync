using BarberSync.Api.Security;
using BarberSync.Application.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Authorize, Route("api/service-orders")]
public sealed class ServiceOrdersController(IServiceOrderService orders, IPaymentService payments) : ControllerBase
{
    [HttpGet, RequirePermission("ServiceOrder.Read")] public Task<IReadOnlyList<ServiceOrderResponse>> List(CancellationToken ct) => orders.ListAsync(ct);
    [HttpGet("{id:guid}"), RequirePermission("ServiceOrder.Read")] public async Task<IActionResult> Get(Guid id,CancellationToken ct) => await orders.GetAsync(id,ct) is { } order ? Ok(order) : NotFound();
    [HttpPost("open"), RequirePermission("ServiceOrder.Create")] public async Task<IActionResult> Open(OpenServiceOrderRequest request,CancellationToken ct) { var order=await orders.OpenAsync(request,ct); return CreatedAtAction(nameof(Get),new{id=order.Id},order); }
    [HttpPost("{id:guid}/items/services"), RequirePermission("ServiceOrder.Update")] public Task<ServiceOrderResponse> AddService(Guid id,AddServiceItemRequest request,CancellationToken ct) => orders.AddServiceAsync(id,request,ct);
    [HttpPost("{id:guid}/items/products"), RequirePermission("ServiceOrder.Update")] public Task<ServiceOrderResponse> AddProduct(Guid id,AddProductItemRequest request,CancellationToken ct) => orders.AddProductAsync(id,request,ct);
    [HttpPut("{id:guid}/items/{itemId:guid}"), RequirePermission("ServiceOrder.Update")] public Task<ServiceOrderResponse> UpdateItem(Guid id,Guid itemId,UpdateOrderItemRequest request,CancellationToken ct) => orders.UpdateItemAsync(id,itemId,request,ct);
    [HttpDelete("{id:guid}/items/{itemId:guid}"), RequirePermission("ServiceOrder.Update")] public Task<ServiceOrderResponse> RemoveItem(Guid id,Guid itemId,CancellationToken ct) => orders.RemoveItemAsync(id,itemId,ct);
    [HttpPost("{id:guid}/discount"), RequirePermission("ServiceOrder.Discount")] public Task<ServiceOrderResponse> Discount(Guid id,ApplyDiscountRequest request,CancellationToken ct) => orders.ApplyDiscountAsync(id,request,ct);
    [HttpPost("{id:guid}/payments"), RequirePermission("Payment.Create")] public Task<PaymentResponse> Pay(Guid id,RegisterPaymentRequest request,CancellationToken ct) => payments.RegisterAsync(id,request,ct);
    [HttpPost("payments/{paymentId:guid}/refund"), RequirePermission("Payment.Refund")] public Task<PaymentResponse> Refund(Guid paymentId,RefundPaymentRequest request,CancellationToken ct) => payments.RefundAsync(paymentId,request,ct);
}
