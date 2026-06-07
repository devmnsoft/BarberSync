using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.PublicWeb.Controllers;

[ApiController]
[Route("PublicApi")]
public class PublicApiController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<PublicApiController> logger) : ControllerBase
{
    [HttpGet("services")]
    public Task<IActionResult> Services() => ProxyGet("/api/public/services", DemoServices());

    [HttpGet("professionals")]
    public Task<IActionResult> Professionals() => ProxyGet("/api/public/professionals", DemoProfessionals());

    [HttpGet("appointments")]
    public Task<IActionResult> Appointments() => ProxyGet("/api/appointments", DemoAppointments());

    [HttpPost("leads")]
    public Task<IActionResult> Leads([FromBody] JsonElement payload) => ProxyPost("/api/public/leads", payload, new { success = true, message = "Lead recebido em modo demonstração.", data = new { protocol = "LEAD-DEMO" } });

    [HttpPost("appointments")]
    public Task<IActionResult> CreateAppointment([FromBody] JsonElement payload) => ProxyPost("/api/public/appointments", payload, new { success = true, message = "Solicitação enviada com sucesso.", data = new { protocol = "PUB-2026-0001", isDemo = true } });

    private async Task<IActionResult> ProxyGet(string path, object fallback)
    {
        try
        {
            var response = await httpClientFactory.CreateClient("BarberSyncApi").GetAsync(BuildUrl(path));
            if (!response.IsSuccessStatusCode) return Ok(DemoEnvelope(fallback, "Dados carregados em modo demonstração."));
            var json = await response.Content.ReadAsStringAsync();
            return Content(json, "application/json", Encoding.UTF8);
        }
        catch (Exception ex) { logger.LogWarning(ex, "Falha PublicApi {Path}", path); return Ok(DemoEnvelope(fallback, "Dados carregados em modo demonstração.")); }
    }
    private async Task<IActionResult> ProxyPost(string path, JsonElement payload, object fallback)
    {
        try
        {
            var response = await httpClientFactory.CreateClient("BarberSyncApi").PostAsync(BuildUrl(path), new StringContent(payload.GetRawText(), Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode) return Ok(fallback);
            return Content(await response.Content.ReadAsStringAsync(), "application/json", Encoding.UTF8);
        }
        catch (Exception ex) { logger.LogWarning(ex, "Falha PublicApi {Path}", path); return Ok(fallback); }
    }
    private string BuildUrl(string path) => $"{(configuration["ApiSettings:BaseUrl"] ?? configuration["ApiBaseUrl"] ?? "http://localhost:8080").TrimEnd('/')}/{path.TrimStart('/')}";
    private static object DemoEnvelope(object data, string message) => new { success = true, message, data, isDemo = true };
    private static bool ResponseLooksEmpty(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return true;
        try
        {
            using var doc = JsonDocument.Parse(json);
            return ElementLooksEmpty(doc.RootElement);
        }
        catch { return false; }
    }
    private static bool ElementLooksEmpty(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array) return element.GetArrayLength() == 0;
        if (element.ValueKind != JsonValueKind.Object) return false;
        if (element.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array && items.GetArrayLength() == 0) return true;
        if (element.TryGetProperty("data", out var data)) return ElementLooksEmpty(data);
        return false;
    }

    private static object[] DemoServices() => new object[] { new { id = "srv-001", name = "Corte Masculino", category = "Barbearia", description = "Corte premium com consultoria de estilo.", price = 45, durationMinutes = 40 }, new { id = "srv-002", name = "Barba Tradicional", category = "Barbearia", description = "Toalha quente e acabamento com navalha.", price = 35, durationMinutes = 30 }, new { id = "srv-003", name = "Corte + Barba", category = "Combo", description = "Experiência completa para ocasiões especiais.", price = 75, durationMinutes = 70 }, new { id = "srv-004", name = "Sobrancelha", category = "Estética", description = "Design rápido para completar o visual.", price = 25, durationMinutes = 20 }, new { id = "srv-005", name = "Hidratação Capilar", category = "Tratamento", description = "Tratamento capilar profissional.", price = 60, durationMinutes = 45 }, new { id = "srv-006", name = "Manicure", category = "Beleza", description = "Cuidado completo para unhas.", price = 40, durationMinutes = 50 } };
    private static object[] DemoProfessionals() => new object[] { new { id = "pro-001", name = "Rafael Barber", specialty = "Fade e barba", rating = 4.9 }, new { id = "pro-002", name = "Lucas Navalha", specialty = "Corte clássico", rating = 4.8 }, new { id = "pro-003", name = "Bruno Estilo", specialty = "Corte social", rating = 4.7 }, new { id = "pro-004", name = "Camila Beauty", specialty = "Visagismo", rating = 4.9 }, new { id = "pro-005", name = "Amanda Nails", specialty = "Nails premium", rating = 4.7 } };
    private static object[] DemoAppointments() => new object[] { new { id = "pub-001", serviceName = "Corte Masculino", professionalName = "Rafael Barber", time = "10:00", status = "Disponível" }, new { id = "pub-002", serviceName = "Combo Corte + Barba", professionalName = "Lucas Navalha", time = "14:30", status = "Disponível" } };
}
