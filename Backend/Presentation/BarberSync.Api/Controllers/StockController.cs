using System.Text.Json;
using BarberSync.Api.Security;
using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Authorize]
[Route("api/stock")]
public sealed class StockController(EnterpriseDataService data, ILogger<StockController> logger) : EnterpriseCrudController(data, logger, "products")
{
    [HttpGet, RequirePermission("Stock.View")] public Task<IActionResult> GetAll(CancellationToken cancellationToken) => List(cancellationToken);
    [HttpGet("critical"), RequirePermission("Stock.View")] public Task<IActionResult> Critical(CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.CriticalStockAsync(cancellationToken), "Estoque crítico carregado com sucesso.")));
    [HttpGet("{id:guid}"), RequirePermission("Stock.View")] public Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) => Get(id, cancellationToken);
    [HttpPost, RequirePermission("Stock.Adjust")] public Task<IActionResult> CreateProduct([FromBody] JsonElement payload, CancellationToken cancellationToken) => Create(payload, cancellationToken);
    [HttpPut("{id:guid}"), RequirePermission("Stock.Adjust")] public Task<IActionResult> UpdateProduct(Guid id, [FromBody] JsonElement payload, CancellationToken cancellationToken) => Update(id, payload, cancellationToken);
    [HttpDelete("{id:guid}"), RequirePermission("Stock.Adjust")] public Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken) => Delete(id, cancellationToken);
    [HttpPost("entry"), RequirePermission("Stock.Entry")] public Task<IActionResult> Entry([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.StockMovementAsync("entry", payload, cancellationToken), "Entrada de estoque registrada.")));
    [HttpPost("exit"), RequirePermission("Stock.Adjust")] public Task<IActionResult> Exit([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.StockMovementAsync("exit", payload, cancellationToken), "Saída de estoque registrada.")));
    [HttpPost("adjustment"), RequirePermission("Stock.Adjust")] public Task<IActionResult> Adjustment([FromBody] JsonElement payload, CancellationToken cancellationToken) => Safe(async () => Ok(Envelope(await data.StockMovementAsync("adjustment", payload, cancellationToken), "Ajuste de estoque registrado.")));
}
