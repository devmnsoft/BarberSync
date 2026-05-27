using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

[Route("AdminApi")]
public class AdminApiController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AdminApiController> logger) : Controller
{
    [HttpGet("dashboard")] public Task<IActionResult> Dashboard() => ProxyGet("/api/dashboard", new { summary = "demo" });
    [HttpGet("services")] public Task<IActionResult> Services() => ProxyGet("/api/services", new[] { new { name = "Corte Masculino", price = 45 } });
    [HttpGet("professionals")] public Task<IActionResult> Professionals() => ProxyGet("/api/professionals", new[] { new { name = "Rafael Barber" } });
    [HttpGet("clients")] public Task<IActionResult> Clients() => ProxyGet("/api/clients", Array.Empty<object>());
    [HttpGet("appointments")] public Task<IActionResult> Appointments() => ProxyGet("/api/appointments", Array.Empty<object>());
    [HttpGet("stock-critical")] public Task<IActionResult> StockCritical() => ProxyGet("/api/stock/critical", Array.Empty<object>());
    [HttpGet("campaigns")] public Task<IActionResult> Campaigns() => ProxyGet("/api/campaigns", Array.Empty<object>());
    [HttpGet("copilot-suggestions")] public Task<IActionResult> Copilot() => ProxyGet("/api/copilot/suggestions", Array.Empty<object>());

    private async Task<IActionResult> ProxyGet(string path, object fallback)
    {
        try { var r = await httpClientFactory.CreateClient().GetAsync($"{(configuration["ApiSettings:BaseUrl"] ?? "http://localhost:8080").TrimEnd('/')}/{path.TrimStart('/')}" ); if (!r.IsSuccessStatusCode) return Ok(fallback); return Content(await r.Content.ReadAsStringAsync(), "application/json", Encoding.UTF8); }
        catch (Exception ex) { logger.LogWarning(ex, "Falha AdminApi {Path}", path); return Ok(fallback); }
    }
}
