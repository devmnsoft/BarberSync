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
    [HttpGet("loyalty")] public Task<IActionResult> Loyalty() => ProxyGet("/api/loyalty/summary", DemoLoyalty());
    [HttpGet("copilot-suggestions")] public Task<IActionResult> CopilotSuggestions() => ProxyGet("/api/copilot/suggestions", DemoCopilotSuggestions());
    [HttpPost("copilot/ask")] public Task<IActionResult> CopilotAsk([FromBody] JsonElement payload) => ProxyPost("/api/copilot/ask", payload, new { success = true, answer = "Modo demonstração: priorize retenção, recompras e ocupação dos horários ociosos." });

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
        kpis = new[] {
            new { label = "Receita hoje", value = 4850m }, new { label = "Receita mês", value = 97800m }, new { label = "Agendamentos hoje", value = 26m },
            new { label = "Clientes ativos", value = 412m }, new { label = "Atendimentos em andamento", value = 7m }, new { label = "Comandas abertas", value = 9m },
            new { label = "Ticket médio", value = 83m }, new { label = "Estoque crítico", value = 4m }, new { label = "Avaliação média", value = 4.8m },
            new { label = "Campanhas ativas", value = 3m }, new { label = "Totem online", value = 1m }, new { label = "Profissionais disponíveis", value = 5m }
        },
        charts = new { weeklyRevenue = new[] { 3200, 4100, 3800, 4500, 4700, 5200, 4850 } },
        appointments = DemoAppointments(), stockCritical = DemoStock(), copilotSuggestions = DemoCopilotSuggestions(),
        alerts = new[] { "Pico previsto entre 18h e 20h", "3 produtos abaixo do estoque mínimo" }
    };

    private static object[] DemoServices() => [
        new { id = 1, name = "Corte Masculino", price = 45, durationMinutes = 40 }, new { id = 2, name = "Barba Tradicional", price = 35, durationMinutes = 30 },
        new { id = 3, name = "Corte + Barba", price = 70, durationMinutes = 60 }, new { id = 4, name = "Sobrancelha", price = 20, durationMinutes = 20 },
        new { id = 5, name = "Hidratação", price = 55, durationMinutes = 45 }, new { id = 6, name = "Manicure", price = 40, durationMinutes = 35 }
    ];

    private static object[] DemoProfessionals() => [
        new { id = 1, name = "Rafael Barber" }, new { id = 2, name = "Lucas Navalha" }, new { id = 3, name = "Bruno Estilo" },
        new { id = 4, name = "Camila Beauty" }, new { id = 5, name = "Amanda Nails" }
    ];

    private static object[] DemoClients() => Enumerable.Range(1, 10).Select(i => new { id = i, name = $"Cliente Demo {i}", status = i % 3 == 0 ? "VIP" : "Ativo" }).ToArray();
    private static object[] DemoAppointments() => Enumerable.Range(1, 8).Select(i => new { id = i, client = $"Cliente Demo {i}", service = i % 2 == 0 ? "Corte + Barba" : "Corte Masculino", time = $"{9 + i:00}:00", status = i % 3 == 0 ? "Em atendimento" : "Confirmado" }).ToArray();
    private static object[] DemoServiceOrders() => Enumerable.Range(1, 5).Select(i => new { code = $"SO-10{i:00}", status = i % 2 == 0 ? "Aberta" : "Em execução", total = 70 + i * 15 }).ToArray();
    private static object[] DemoProducts() => Enumerable.Range(1, 10).Select(i => new { id = i, name = $"Produto Demo {i}", stock = 5 + i, min = 8 }).ToArray();
    private static object[] DemoStock() => [ new { name = "Lâmina Platinum", current = 6, minimum = 20 }, new { name = "Toalha Premium", current = 10, minimum = 25 }, new { name = "Pomada Modeladora", current = 7, minimum = 15 }, new { name = "Shampoo Anticaspa", current = 5, minimum = 10 } ];
    private static object[] DemoCampaigns() => [ new { name = "Volte e ganhe", channel = "WhatsApp", status = "Ativa" }, new { name = "Combo do mês", channel = "Instagram", status = "Ativa" }, new { name = "Aniversariantes", channel = "SMS", status = "Agendada" } ];
    private static object[] DemoCoupons() => [ new { code = "BARBA10", discount = 10, status = "Ativo" }, new { code = "VIP15", discount = 15, status = "Ativo" }, new { code = "WELCOME5", discount = 5, status = "Ativo" } ];
    private static object[] DemoReviews() => [ new { client = "Marcos", rating = 5, comment = "Atendimento excelente" }, new { client = "Thiago", rating = 4, comment = "Ótimo acabamento" }, new { client = "Eduardo", rating = 5, comment = "Voltarei com certeza" } ];
    private static object DemoLoyalty() => new { pointsDistributed = 1800, activeMembers = 132, cashbackMonth = 2340 };
    private static object[] DemoCopilotSuggestions() => [ "Reforçar campanha de retorno para clientes inativos há 45 dias.", "Aumentar escala no horário de pico entre 18h e 20h.", "Criar cupom de recompra para serviços com baixa ocupação." ];
}
