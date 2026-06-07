using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(EnterpriseDataService data, ILogger<DashboardController> logger) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(new { success = true, message = "Dashboard carregado com dados reais.", data = await data.DashboardAsync(cancellationToken), errors = Array.Empty<object>() });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao carregar dashboard.");
            return StatusCode(500, new { success = false, message = "Erro interno ao processar a solicitação.", data = (object?)null, errors = Array.Empty<object>() });
        }
    }
}
