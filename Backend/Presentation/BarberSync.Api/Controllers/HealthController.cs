using BarberSync.Api.Services.Enterprise;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController(IBarberSchemaInitializer schemaInitializer) : ControllerBase
{
    [HttpGet("database")]
    public async Task<IActionResult> Database(CancellationToken cancellationToken)
    {
        var result = await schemaInitializer.CheckHealthAsync(cancellationToken);
        var payload = new
        {
            success = result.Success,
            databaseConnected = result.DatabaseConnected,
            schemaReady = result.SchemaReady,
            message = result.Message,
            database = result.Database,
            schema = result.Schema,
            environment = result.Environment,
            step = result.Step
        };

        return Ok(payload);
    }
}
