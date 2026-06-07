using System.Text.Json;
using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/stock")]
public sealed class StockController(EnterpriseDataService data, ILogger<StockController> logger) : EnterpriseCrudController(data, logger, "products")
{
    [HttpGet] public Task<IActionResult> GetAll(CancellationToken cancellationToken) => List(cancellationToken);
    [HttpGet("critical")] public Task<IActionResult> Critical(CancellationToken cancellationToken) => List(cancellationToken);
    [HttpGet("{id:guid}")] public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) => Get(id, cancellationToken);
    [HttpPost] public Task<IActionResult> CreateProduct([FromBody] JsonElement payload, CancellationToken cancellationToken) => Create(payload, cancellationToken);
    [HttpPut("{id:guid}")] public Task<IActionResult> UpdateProduct(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken) => Update(id, payload, cancellationToken);
    [HttpDelete("{id:guid}")] public Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken) => Delete(id, cancellationToken);
    [HttpPost("entry")] public Task<IActionResult> Entry([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.StockMovementAsync("entry", payload, cancellationToken), "Entrada de estoque registrada.")));
    [HttpPost("exit")] public Task<IActionResult> Exit([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.StockMovementAsync("exit", payload, cancellationToken), "Saída de estoque registrada.")));
    [HttpPost("adjustment")] public Task<IActionResult> Adjustment([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.StockMovementAsync("adjustment", payload, cancellationToken), "Ajuste de estoque registrado.")));
}
