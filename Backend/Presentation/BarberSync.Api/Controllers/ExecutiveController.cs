using System.Text;
using BarberSync.Api.Services.Executive;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController, Route("api/executive"), Authorize]
public sealed class ExecutiveController(ExecutiveInsightsService insights, ILogger<ExecutiveController> logger) : ControllerBase
{
    [HttpGet("owner"), Authorize(Roles = "SuperAdmin,Owner,Admin")]
    public Task<IActionResult> Owner(CancellationToken ct) => Execute(() => insights.OwnerAsync(ct));

    [HttpGet("reception"), Authorize(Roles = "SuperAdmin,Owner,Admin,Receptionist")]
    public Task<IActionResult> Reception(CancellationToken ct) => Execute(() => insights.ReceptionAsync(ct));

    [HttpGet("export.csv"), Authorize(Roles = "SuperAdmin,Owner,Admin,Financial")]
    public async Task<IActionResult> Csv(CancellationToken ct)
    {
        var data = await insights.OwnerAsync(ct); await insights.AuditAsync("ReportExportedCsv", "Dashboard executivo exportado em CSV.", ct);
        var json = System.Text.Json.JsonSerializer.Serialize(data).Replace("\"", "\"\"");
        return File(Encoding.UTF8.GetBytes("sep=;\r\nrelatorio;geradoEm;dados\r\nexecutivo;" + DateTime.UtcNow.ToString("O") + ";\"" + json + "\"\r\n"), "text/csv; charset=utf-8", "barbersync-executivo.csv");
    }

    private async Task<IActionResult> Execute(Func<Task<object>> action)
    {
        try { return Ok(new { success = true, data = await action(), traceId = HttpContext.TraceIdentifier }); }
        catch (UnauthorizedAccessException) { return Unauthorized(new ProblemDetails { Title = "Escopo inválido", Detail = "Tenant e unidade são obrigatórios.", Extensions = { ["traceId"] = HttpContext.TraceIdentifier } }); }
        catch (Exception exception) { logger.LogError(exception, "Falha na visão executiva {TraceId}", HttpContext.TraceIdentifier); return Problem(statusCode: 500, title: "Não foi possível gerar a visão executiva.", extensions: new Dictionary<string, object?> { ["traceId"] = HttpContext.TraceIdentifier }); }
    }
}
