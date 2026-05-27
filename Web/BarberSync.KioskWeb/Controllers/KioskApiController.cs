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
            var client = httpClientFactory.CreateClient();
            var response = await client.GetAsync(BuildApiUrl(path));
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("KioskApi proxy GET {Path} falhou com status {StatusCode}", path, response.StatusCode);
                return Ok(new { success = true, message = fallbackMessage, data = fallbackData });
            }

            return Content(await response.Content.ReadAsStringAsync(), "application/json", Encoding.UTF8);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "KioskApi proxy GET {Path} lançou exceção", path);
            return Ok(new { success = true, message = fallbackMessage, data = fallbackData });
        }
    }

    private async Task<IActionResult> ProxyPost(string path, JsonElement payload, object fallback)
    {
        try
        {
            var client = httpClientFactory.CreateClient();
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
            logger.LogError(ex, "KioskApi proxy POST {Path} lançou exceção", path);
            return Ok(fallback);
        }
    }

    private string BuildApiUrl(string path)
    {
        var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:8080";
        return $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }

    private static object[] DemoServices() =>
    [
        new { id = "demo-corte", name = "Corte Masculino", category = "Barbearia", description = "Corte moderno com acabamento profissional.", price = 45.00, durationMinutes = 40, icon = "✂️", isAvailable = true, isDemo = true },
        new { id = "demo-barba", name = "Barba Tradicional", category = "Barbearia", description = "Barba alinhada com toalha quente e navalha.", price = 35.00, durationMinutes = 30, icon = "🪒", isAvailable = true, isDemo = true }
    ];

    private static object[] DemoProfessionals() => [new { name = "Rafael Barber", specialty = "Fade", estimatedWaitMinutes = 10, isDemo = true }];
}
