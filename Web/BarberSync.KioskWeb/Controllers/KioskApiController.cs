using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.KioskWeb.Controllers;

[Route("KioskApi")]
public class KioskApiController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<KioskApiController> logger) : Controller
{
    [HttpGet("services")]
    public Task<IActionResult> Services([FromQuery] string? deviceCode = "KIOSK-DEMO-001")
        => ProxyGet($"/api/kiosk/services?deviceCode={Uri.EscapeDataString(string.IsNullOrWhiteSpace(deviceCode) ? "KIOSK-DEMO-001" : deviceCode.Trim())}", DemoServices(), "Serviços carregados em modo demonstração.");

    [HttpGet("professionals")]
    public Task<IActionResult> Professionals([FromQuery] string? serviceId, [FromQuery] string? deviceCode)
        => ProxyGet($"/api/kiosk/professionals?serviceId={Uri.EscapeDataString(serviceId ?? string.Empty)}&deviceCode={Uri.EscapeDataString(deviceCode ?? "KIOSK-DEMO-001")}", DemoProfessionals(), "Profissionais carregados em modo demonstração.");

    [HttpGet("operations-snapshot")]
    public Task<IActionResult> OperationsSnapshot()
        => ProxyGet("/api/futuristic-automation/operations-snapshot", DemoOperationsSnapshot(), "Snapshot operacional carregado em modo demonstração.");

    [HttpPost("client/find-by-phone")]
    public Task<IActionResult> FindByPhone([FromBody] JsonElement payload) => ProxyPost("/api/kiosk/client/find-by-phone", payload, new { success = true, message = "Cliente não encontrado em produção. Modo demonstração ativo.", data = new { name = "Cliente Demo", phone = "(11) 99999-9999", isDemo = true } });

    [HttpPost("client/quick-register")]
    public Task<IActionResult> QuickRegister([FromBody] JsonElement payload) => ProxyPost("/api/kiosk/client/quick-register", payload, new { success = true, message = "Cadastro rápido realizado em modo demonstração.", data = new { protocol = "DEMO-001", isDemo = true } });

    [HttpPost("payment/mock")]
    public Task<IActionResult> MockPayment([FromBody] JsonElement payload) => ProxyPost("/api/kiosk/payment/mock", payload, new { success = true, message = "Pagamento simulado aprovado.", data = new { status = "APPROVED", isDemo = true } });

    [HttpPost("review")]
    public Task<IActionResult> Review([FromBody] JsonElement payload) => ProxyPost("/api/kiosk/review", payload, new { success = true, message = "Avaliação recebida em modo demonstração.", data = new { isDemo = true } });

    private async Task<IActionResult> ProxyGet(string path, object fallbackData, string fallbackMessage)
    {
        try
        {
            var client = httpClientFactory.CreateClient("BarberSyncApi");
            var response = await client.GetAsync(BuildApiUrl(path));
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("KioskApi proxy GET {Path} falhou com status {StatusCode}", path, response.StatusCode);
                return Ok(new { success = true, message = fallbackMessage, data = fallbackData });
            }

            var json = await response.Content.ReadAsStringAsync();
            if (!ResponseLooksEmpty(json))
            {
                return Content(json, "application/json", Encoding.UTF8);
            }

            logger.LogWarning("KioskApi proxy GET {Path} retornou payload vazio", path);
            return Ok(new { success = true, message = fallbackMessage, data = fallbackData });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "KioskApi proxy GET {Path} lançou exceção. Usando fallback demo.", path);
            return Ok(new { success = true, message = fallbackMessage, data = fallbackData });
        }
    }

    private async Task<IActionResult> ProxyPost(string path, JsonElement payload, object fallback)
    {
        try
        {
            var client = httpClientFactory.CreateClient("BarberSyncApi");
            var response = await client.PostAsync(BuildApiUrl(path), new StringContent(payload.GetRawText(), Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("KioskApi proxy POST {Path} falhou com status {StatusCode}", path, response.StatusCode);
                return Ok(fallback);
            }

            return Content(await response.Content.ReadAsStringAsync(), "application/json", Encoding.UTF8);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "KioskApi proxy POST {Path} lançou exceção. Usando fallback demo.", path);
            return Ok(fallback);
        }
    }

    private string BuildApiUrl(string path)
    {
        var baseUrl = configuration["ApiSettings:BaseUrl"] ?? configuration["ApiBaseUrl"] ?? "http://localhost:8080";
        return $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }

    private static bool ResponseLooksEmpty(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return true;
        try
        {
            using var document = JsonDocument.Parse(json);
            return ElementLooksEmpty(document.RootElement);
        }
        catch
        {
            return false;
        }
    }

    private static bool ElementLooksEmpty(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array) return element.GetArrayLength() == 0;
        if (element.ValueKind != JsonValueKind.Object) return false;
        if (element.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array && items.GetArrayLength() == 0) return true;
        if (element.TryGetProperty("data", out var data)) return ElementLooksEmpty(data);
        return false;
    }


    private static object DemoOperationsSnapshot() => new
    {
        generatedAtUtc = DateTime.UtcNow,
        kpis = new Dictionary<string, decimal>
        {
            ["occupancy"] = 81.4m,
            ["nps"] = 89.3m,
            ["automation_coverage"] = 78.1m,
            ["prediction_accuracy"] = 92.8m
        },
        notifications = new object[]
        {
            new { type = "stock-critical", channel = "operations", message = "Estoque crítico de pomada matte. Reposição sugerida em 45 min.", scheduledAtUtc = DateTime.UtcNow.AddMinutes(1), isDemo = true },
            new { type = "service-delay", channel = "totem", message = "Cliente avisado sobre espera estimada de 8 minutos.", scheduledAtUtc = DateTime.UtcNow.AddMinutes(2), isDemo = true }
        },
        professionals = DemoProfessionals(),
        isDemo = true
    };

    private static object[] DemoServices() => new object[]
    {
        new { id = "demo-corte", name = "Corte Masculino", category = "Barbearia", description = "Corte moderno com acabamento profissional.", price = 45.00, durationMinutes = 40, icon = "✂️", isAvailable = true, isDemo = true },
        new { id = "demo-barba", name = "Barba Tradicional", category = "Barbearia", description = "Barba alinhada com toalha quente e navalha.", price = 35.00, durationMinutes = 30, icon = "🪒", isAvailable = true, isDemo = true },
        new { id = "demo-combo", name = "Corte + Barba", category = "Combo", description = "Atendimento completo com preço especial.", price = 70.00, durationMinutes = 60, icon = "💈", isAvailable = true, isDemo = true },
        new { id = "demo-sobrancelha", name = "Sobrancelha", category = "Estética", description = "Design rápido para completar o visual.", price = 25.00, durationMinutes = 20, icon = "✨", isAvailable = true, isDemo = true },
        new { id = "demo-hidratacao", name = "Hidratação Capilar", category = "Estética", description = "Tratamento capilar profissional para brilho e recuperação.", price = 60.00, durationMinutes = 45, icon = "💧", isAvailable = true, isDemo = true },
        new { id = "demo-manicure", name = "Manicure", category = "Beleza", description = "Cuidado completo para unhas em atendimento rápido.", price = 40.00, durationMinutes = 50, icon = "💅", isAvailable = true, isDemo = true }
    };

    private static object[] DemoProfessionals() => new object[]
    {
        new { id = "pro-rafael", name = "Rafael Barber", specialty = "Fade e barba", rating = 4.9, estimatedWaitMinutes = 10, isDemo = true },
        new { id = "pro-lucas", name = "Lucas Navalha", specialty = "Corte clássico", rating = 4.8, estimatedWaitMinutes = 15, isDemo = true },
        new { id = "pro-bruno", name = "Bruno Estilo", specialty = "Corte social", rating = 4.7, estimatedWaitMinutes = 18, isDemo = true },
        new { id = "pro-camila", name = "Camila Beauty", specialty = "Visagismo e estética", rating = 4.9, estimatedWaitMinutes = 20, isDemo = true },
        new { id = "pro-amanda", name = "Amanda Nails", specialty = "Manicure premium", rating = 4.8, estimatedWaitMinutes = 12, isDemo = true }
    };
}
