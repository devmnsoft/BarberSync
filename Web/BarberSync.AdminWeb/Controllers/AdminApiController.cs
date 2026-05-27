using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

[ApiController]
[Route("AdminApi")]
public class AdminApiController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AdminApiController> logger) : ControllerBase
{
    [HttpGet("dashboard")] public Task<IActionResult> Dashboard() => ProxyGet("/api/dashboard/summary", DemoDashboard());
    [HttpGet("services")] public Task<IActionResult> Services() => ProxyGet("/api/services", DemoServices());
    [HttpGet("professionals")] public Task<IActionResult> Professionals() => ProxyGet("/api/professionals", DemoProfessionals());
    [HttpGet("clients")] public Task<IActionResult> Clients() => ProxyGet("/api/clients", DemoClients());
    [HttpGet("appointments")] public Task<IActionResult> Appointments() => ProxyGet("/api/appointments", DemoAppointments());
    [HttpGet("service-orders")] public Task<IActionResult> ServiceOrders() => ProxyGet("/api/service-orders", DemoServiceOrders());
    [HttpGet("products")] public Task<IActionResult> Products() => ProxyGet("/api/products", DemoProducts());
    [HttpGet("stock-critical")] public Task<IActionResult> StockCritical() => ProxyGet("/api/stock/critical", DemoStock());
    [HttpGet("campaigns")] public Task<IActionResult> Campaigns() => ProxyGet("/api/campaigns", DemoCampaigns());
    [HttpGet("coupons")] public Task<IActionResult> Coupons() => ProxyGet("/api/coupons", DemoCoupons());
    [HttpGet("reviews")] public Task<IActionResult> Reviews() => ProxyGet("/api/reviews", DemoReviews());
    [HttpGet("loyalty")] public Task<IActionResult> Loyalty() => ProxyGet("/api/loyalty/summary", new { pointsDistributed = 1800, activeMembers = 132, cashbackMonth = 2340 });
    [HttpGet("copilot-suggestions")] public Task<IActionResult> CopilotSuggestions() => ProxyGet("/api/copilot/suggestions", new[] { "Reforçar campanha de retorno para clientes inativos há 45 dias.", "Aumentar escala no horário de pico entre 18h e 20h." });
    [HttpPost("copilot/ask")] public Task<IActionResult> CopilotAsk([FromBody] JsonElement payload) => ProxyPost("/api/copilot/ask", payload, new { success = true, answer = "Modo demonstração: priorize campanhas de retenção e reativação." });

    private async Task<IActionResult> ProxyGet(string path, object fallback)
    {
        try
        {
            var response = await httpClientFactory.CreateClient().GetAsync(BuildUrl(path));
            if (!response.IsSuccessStatusCode) return Ok(fallback);
            return Content(await response.Content.ReadAsStringAsync(), "application/json", Encoding.UTF8);
        }
        catch (Exception ex) { logger.LogWarning(ex, "Falha AdminApi GET {Path}", path); return Ok(fallback); }
    }

    private async Task<IActionResult> ProxyPost(string path, JsonElement payload, object fallback)
    {
        try
        {
            var response = await httpClientFactory.CreateClient().PostAsync(BuildUrl(path), new StringContent(payload.GetRawText(), Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode) return Ok(fallback);
            return Content(await response.Content.ReadAsStringAsync(), "application/json", Encoding.UTF8);
        }
        catch (Exception ex) { logger.LogWarning(ex, "Falha AdminApi POST {Path}", path); return Ok(fallback); }
    }

    private string BuildUrl(string path) => $"{(configuration["ApiSettings:BaseUrl"] ?? "http://localhost:8080").TrimEnd('/')}/{path.TrimStart('/')}";

    private static object DemoDashboard() => new
    {
        kpis = new[] { new { label = "Receita hoje", value = 4850 }, new { label = "Agendamentos", value = 26 }, new { label = "Clientes ativos", value = 412 } },
        charts = new { weeklyRevenue = new[] { 3200, 4100, 3800, 4500, 4700, 5200, 4850 } },
        appointments = DemoAppointments(),
        stockCritical = DemoStock(),
        copilotSuggestions = new[] { "Reforçar equipe no pico das 19h", "Ativar cupom para clientes inativos" }
    };

    private static object[] DemoServices() =>
    [
        new { name = "Corte Masculino", price = 45, durationMinutes = 40 },
        new { name = "Barba Tradicional", price = 35, durationMinutes = 30 },
        new { name = "Corte + Barba", price = 70, durationMinutes = 60 },
        new { name = "Sobrancelha", price = 20, durationMinutes = 20 },
        new { name = "Hidratação", price = 55, durationMinutes = 45 },
        new { name = "Manicure", price = 40, durationMinutes = 35 }
    ];
    private static object[] DemoProfessionals() => [ new { name = "Rafael Barber" }, new { name = "Lucas Navalha" }, new { name = "Bruno Estilo" }, new { name = "Camila Beauty" }, new { name = "Amanda Nails" } ];
    private static object[] DemoClients() => [ new { name = "Carlos Souza", status = "VIP" }, new { name = "João Lima", status = "Ativo" } ];
    private static object[] DemoAppointments() => [ new { client = "Carlos Souza", service = "Corte + Barba", time = "14:30", status = "Confirmado" } ];
    private static object[] DemoServiceOrders() => [ new { code = "SO-1042", status = "Em execução", total = 120.0 } ];
    private static object[] DemoProducts() => [ new { name = "Pomada Matte", stock = 24 }, new { name = "Shampoo Anticaspa", stock = 9 } ];
    private static object[] DemoStock() => [ new { name = "Lâmina Platinum", current = 6, minimum = 20 }, new { name = "Toalha Premium", current = 10, minimum = 25 } ];
    private static object[] DemoCampaigns() => [ new { name = "Volte e ganhe", channel = "WhatsApp", status = "Ativa" } ];
    private static object[] DemoCoupons() => [ new { code = "BARBA10", discount = 10, status = "Ativo" } ];
    private static object[] DemoReviews() => [ new { client = "Marcos", rating = 5, comment = "Atendimento excelente" } ];
}
