using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.PublicWeb.Controllers;

[Route("PublicApi")]
public class PublicApiController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<PublicApiController> logger) : Controller
{
    [HttpGet("services")]
    public Task<IActionResult> Services() => ProxyGet("/api/services", DemoServices());

    [HttpGet("professionals")]
    public Task<IActionResult> Professionals() => ProxyGet("/api/professionals", DemoProfessionals());

    [HttpGet("appointments")]
    public Task<IActionResult> Appointments() => ProxyGet("/api/appointments", DemoAppointments());

    [HttpPost("leads")]
    public Task<IActionResult> Leads([FromBody] JsonElement payload) => ProxyPost("/api/leads", payload, new { success = true, message = "Lead recebido em modo demonstração.", data = new { protocol = "LEAD-DEMO" } });

    [HttpPost("appointments")]
    public Task<IActionResult> CreateAppointment([FromBody] JsonElement payload) => ProxyPost("/api/appointments", payload, new { success = true, message = "Solicitação enviada com sucesso.", data = new { protocol = "PUB-2026-0001", isDemo = true } });

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
    private string BuildUrl(string path) => $"{(configuration["ApiSettings:BaseUrl"] ?? configuration["ApiBaseUrl"] ?? "http://localhost:8080").TrimEnd('/')}/{path.TrimStart('/')}";

    private static object[] DemoServices() => new object[] { new { id = "srv-001", name = "Corte Masculino", category = "Barbearia", description = "Corte premium com consultoria de estilo.", price = 45, durationMinutes = 40 }, new { id = "srv-002", name = "Barba Tradicional", category = "Barbearia", description = "Toalha quente e acabamento com navalha.", price = 35, durationMinutes = 30 }, new { id = "srv-003", name = "Combo Corte + Barba", category = "Combo", description = "Experiência completa para ocasiões especiais.", price = 70, durationMinutes = 60 }, new { id = "srv-004", name = "Hidratação Premium", category = "Tratamento", description = "Tratamento capilar profissional.", price = 55, durationMinutes = 45 } };
    private static object[] DemoProfessionals() => new object[] { new { id = "pro-001", name = "Rafael Barber", specialty = "Fade e barba", rating = 4.9 }, new { id = "pro-002", name = "Lucas Navalha", specialty = "Corte clássico", rating = 4.8 }, new { id = "pro-003", name = "Camila Beauty", specialty = "Visagismo", rating = 4.9 }, new { id = "pro-004", name = "Amanda Nails", specialty = "Nails premium", rating = 4.7 } };
    private static object[] DemoAppointments() => new object[] { new { id = "pub-001", serviceName = "Corte Masculino", professionalName = "Rafael Barber", time = "10:00", status = "Disponível" }, new { id = "pub-002", serviceName = "Combo Corte + Barba", professionalName = "Lucas Navalha", time = "14:30", status = "Disponível" } };
}
