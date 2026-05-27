using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.PublicWeb.Controllers;

[Route("PublicApi")]
public class PublicApiController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<PublicApiController> logger) : Controller
{
    [HttpGet("services")]
    public Task<IActionResult> Services() => ProxyGet("/api/services", new[] { new { name = "Corte Masculino", description = "Atendimento premium.", price = 45 } });

    [HttpGet("professionals")]
    public Task<IActionResult> Professionals() => ProxyGet("/api/professionals", new[] { new { name = "Rafael Barber" } });


    [HttpPost("leads")]
    public Task<IActionResult> Leads([FromBody] JsonElement payload) => ProxyPost("/api/leads", payload, new { success = true, message = "Lead recebido em modo demonstração.", data = new { protocol = "LEAD-DEMO" } });

    [HttpPost("appointments")]
    public Task<IActionResult> Appointments([FromBody] JsonElement payload) => ProxyPost("/api/appointments", payload, new { success = true, message = "Solicitação recebida em modo demonstração.", data = new { protocol = "PUB-DEMO" } });

    private async Task<IActionResult> ProxyGet(string path, object fallback)
    {
        try
        {
            var response = await httpClientFactory.CreateClient().GetAsync(BuildUrl(path));
            if (!response.IsSuccessStatusCode) return Ok(fallback);
            return Content(await response.Content.ReadAsStringAsync(), "application/json", Encoding.UTF8);
        }
        catch (Exception ex) { logger.LogWarning(ex, "Falha PublicApi {Path}", path); return Ok(fallback); }
    }
    private async Task<IActionResult> ProxyPost(string path, JsonElement payload, object fallback)
    {
        try
        {
            var response = await httpClientFactory.CreateClient().PostAsync(BuildUrl(path), new StringContent(payload.GetRawText(), Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode) return Ok(fallback);
            return Content(await response.Content.ReadAsStringAsync(), "application/json", Encoding.UTF8);
        }
        catch (Exception ex) { logger.LogWarning(ex, "Falha PublicApi {Path}", path); return Ok(fallback); }
    }
    private string BuildUrl(string path) => $"{(configuration["ApiSettings:BaseUrl"] ?? "http://localhost:8080").TrimEnd('/')}/{path.TrimStart('/')}";
}
