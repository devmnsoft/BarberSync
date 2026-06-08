using System.Text.Json;
using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/service-orders")]
public sealed class ServiceOrdersController(EnterpriseDataService data, ILogger<ServiceOrdersController> logger) : EnterpriseCrudController(data, logger, "service-orders")
{
    [HttpGet] public Task<IActionResult> GetAll(CancellationToken cancellationToken) => List(cancellationToken);
    [HttpGet("{id:guid}")] public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) => Get(id, cancellationToken);
    [HttpPost] public Task<IActionResult> CreateOrder([FromBody] JsonElement payload, CancellationToken cancellationToken) => Create(payload, cancellationToken);
    [HttpPost("open")] public Task<IActionResult> Open([FromBody] JsonElement payload, CancellationToken cancellationToken) => Create(payload, cancellationToken);
    [HttpPut("{id:guid}")] public Task<IActionResult> UpdateOrder(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken) => Update(id, payload, cancellationToken);
    [HttpDelete("{id:guid}")] public Task<IActionResult> DeleteOrder(Guid id, CancellationToken cancellationToken) => Delete(id, cancellationToken);
    [HttpPost("{id:guid}/add-service")] public Task<IActionResult> AddService(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.CreateAsync("service_order_items", payload, cancellationToken), "Serviço adicionado à comanda.")));
    [HttpPost("{id:guid}/add-product")] public Task<IActionResult> AddProduct(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.CreateAsync("service_order_items", payload, cancellationToken), "Produto adicionado à comanda.")));
    [HttpPost("{id:guid}/apply-discount")] public Task<IActionResult> ApplyDiscount(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken) => Update(id, payload, cancellationToken);
    [HttpPost("{id:guid}/apply-coupon")] public Task<IActionResult> ApplyCoupon(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken) => Update(id, payload, cancellationToken);
    [HttpPost("{id:guid}/apply-cashback")] public Task<IActionResult> ApplyCashback(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken) => Update(id, payload, cancellationToken);
    [HttpPost("{id:guid}/pay")] public Task<IActionResult> Pay(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.PayServiceOrderAsync(id, payload, cancellationToken), "Pagamento registrado com sucesso.")));
    [HttpPost("{id:guid}/close")] public Task<IActionResult> Close(Guid id, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.ChangeServiceOrderStatusAsync(id, "Closed", cancellationToken), "Comanda fechada com sucesso.")));
}
