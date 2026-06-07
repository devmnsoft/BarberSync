using System.Text.Json;
using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/full-service-flow")]
public sealed class FullServiceFlowController(EnterpriseDataService data, ILogger<FullServiceFlowController> logger) : ControllerBase
{
    [HttpGet("snapshot")]
    public async Task<IActionResult> Snapshot(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(new { success = true, message = "Fluxo completo carregado com dados reais.", data = new { clients = await data.ListAsync("clients", cancellationToken), services = await data.ListAsync("services", cancellationToken), professionals = await data.ListAsync("professionals", cancellationToken), products = await data.ListAsync("products", cancellationToken), isDemo = false }, errors = Array.Empty<object>() });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao carregar FullServiceFlow.");
            return StatusCode(500, new { success = false, message = "Erro interno ao processar a solicitação.", data = (object?)null, errors = Array.Empty<object>() });
        }
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] JsonElement payload, CancellationToken cancellationToken)
    {
        try
        {
            var order = await data.CreateAsync("service-orders", payload, cancellationToken);
            return Ok(new { success = true, message = "FullServiceFlow registrado via API real.", data = new { order, receiptNumber = $"REC-{DateTime.UtcNow:yyyyMMddHHmmss}", cashbackGenerated = true, isDemo = false }, errors = Array.Empty<object>() });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao executar FullServiceFlow.");
            return StatusCode(500, new { success = false, message = "Erro interno ao processar a solicitação.", data = (object?)null, errors = Array.Empty<object>() });
        }
    }
}
