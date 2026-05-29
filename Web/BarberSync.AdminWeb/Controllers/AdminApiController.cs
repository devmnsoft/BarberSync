using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

[ApiController]
[Route("AdminApi")]
public class AdminApiController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AdminApiController> logger) : ControllerBase
{
    [HttpGet("dashboard")] public Task<IActionResult> Dashboard() => ProxyGet("/api/dashboard/summary", DemoDashboard());
    [HttpGet("clients")] public Task<IActionResult> Clients() => ProxyGet("/api/clients", DemoClients());
    [HttpGet("professionals")] public Task<IActionResult> Professionals() => ProxyGet("/api/professionals", DemoProfessionals());
    [HttpGet("services")] public Task<IActionResult> Services() => ProxyGet("/api/services", DemoServices());
    [HttpGet("appointments")] public Task<IActionResult> Appointments() => ProxyGet("/api/appointments", DemoAppointments());
    [HttpGet("service-orders")] public Task<IActionResult> ServiceOrders() => ProxyGet("/api/service-orders", DemoServiceOrders());
    [HttpGet("products")] public Task<IActionResult> Products() => ProxyGet("/api/products", DemoProducts());
    [HttpGet("stock-critical")] public Task<IActionResult> StockCritical() => ProxyGet("/api/stock/critical", DemoStock());
    [HttpGet("campaigns")] public Task<IActionResult> Campaigns() => ProxyGet("/api/campaigns", DemoCampaigns());
    [HttpGet("coupons")] public Task<IActionResult> Coupons() => ProxyGet("/api/coupons", DemoCoupons());
    [HttpGet("reviews")] public Task<IActionResult> Reviews() => ProxyGet("/api/reviews", DemoReviews());
    [HttpGet("loyalty")] public Task<IActionResult> Loyalty() => ProxyGet("/api/loyalty/summary", DemoLoyalty());
    [HttpGet("copilot-suggestions")] public Task<IActionResult> CopilotSuggestions() => ProxyGet("/api/copilot/suggestions", DemoCopilotSuggestions());

    [HttpPost("clients")] public Task<IActionResult> CreateClient([FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, "/api/clients", payload, DemoMutation("Cliente criado em modo demonstração.", payload));
    [HttpPut("clients/{id}")] public Task<IActionResult> UpdateClient(string id, [FromBody] JsonElement payload) => ProxySend(HttpMethod.Put, $"/api/clients/{Uri.EscapeDataString(id)}", payload, DemoMutation("Cliente atualizado em modo demonstração.", payload, id));
    [HttpDelete("clients/{id}")] public Task<IActionResult> DeleteClient(string id) => ProxyDelete($"/api/clients/{Uri.EscapeDataString(id)}", DemoMutation("Cliente removido em modo demonstração.", id));

    [HttpPost("professionals")] public Task<IActionResult> CreateProfessional([FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, "/api/professionals", payload, DemoMutation("Profissional criado em modo demonstração.", payload));
    [HttpPut("professionals/{id}")] public Task<IActionResult> UpdateProfessional(string id, [FromBody] JsonElement payload) => ProxySend(HttpMethod.Put, $"/api/professionals/{Uri.EscapeDataString(id)}", payload, DemoMutation("Profissional atualizado em modo demonstração.", payload, id));
    [HttpDelete("professionals/{id}")] public Task<IActionResult> DeleteProfessional(string id) => ProxyDelete($"/api/professionals/{Uri.EscapeDataString(id)}", DemoMutation("Profissional removido em modo demonstração.", id));

    [HttpPost("services")] public Task<IActionResult> CreateService([FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, "/api/services", payload, DemoMutation("Serviço criado em modo demonstração.", payload));
    [HttpPut("services/{id}")] public Task<IActionResult> UpdateService(string id, [FromBody] JsonElement payload) => ProxySend(HttpMethod.Put, $"/api/services/{Uri.EscapeDataString(id)}", payload, DemoMutation("Serviço atualizado em modo demonstração.", payload, id));
    [HttpDelete("services/{id}")] public Task<IActionResult> DeleteService(string id) => ProxyDelete($"/api/services/{Uri.EscapeDataString(id)}", DemoMutation("Serviço removido em modo demonstração.", id));

    [HttpPost("appointments")] public Task<IActionResult> CreateAppointment([FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, "/api/appointments", payload, DemoMutation("Agendamento criado em modo demonstração.", payload));
    [HttpPost("service-orders/open")] public Task<IActionResult> OpenServiceOrder([FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, "/api/service-orders/open", payload, DemoMutation("Comanda aberta em modo demonstração.", payload));
    [HttpPost("service-orders/{id}/pay")] public Task<IActionResult> PayServiceOrder(string id, [FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, $"/api/service-orders/{Uri.EscapeDataString(id)}/pay", payload, DemoMutation("Pagamento da comanda aprovado em modo demonstração.", payload, id));
    [HttpPost("service-orders/{id}/close")] public Task<IActionResult> CloseServiceOrder(string id, [FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, $"/api/service-orders/{Uri.EscapeDataString(id)}/close", payload, DemoMutation("Comanda fechada em modo demonstração.", payload, id));
    [HttpPost("payments/mock")] public Task<IActionResult> MockPayment([FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, "/api/payments/mock", payload, DemoMutation("Pagamento mock aprovado.", payload));
    [HttpPost("stock/entry")] public Task<IActionResult> StockEntry([FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, "/api/stock/entry", payload, DemoMutation("Entrada de estoque registrada em modo demonstração.", payload));
    [HttpPost("campaigns")] public Task<IActionResult> CreateCampaign([FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, "/api/campaigns", payload, DemoMutation("Campanha criada em modo demonstração.", payload));
    [HttpPost("coupons")] public Task<IActionResult> CreateCoupon([FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, "/api/coupons", payload, DemoMutation("Cupom criado em modo demonstração.", payload));
    [HttpPost("copilot/ask")] public Task<IActionResult> CopilotAsk([FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, "/api/copilot/ask", payload, new { success = true, isDemo = true, answer = "Priorize retorno de clientes inativos, combos de alto ticket e escala extra entre 18h e 20h." });

    private async Task<IActionResult> ProxyGet(string path, object fallback)
    {
        try
        {
            var response = await httpClientFactory.CreateClient("BarberSyncApi").GetAsync(BuildUrl(path));
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("AdminApi GET {Path} retornou {StatusCode}. Usando fallback demo.", path, response.StatusCode);
                return Ok(fallback);
            }
            return Content(await response.Content.ReadAsStringAsync(), "application/json", Encoding.UTF8);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha AdminApi GET {Path}. Usando fallback demo.", path);
            return Ok(fallback);
        }
    }

    private Task<IActionResult> ProxySend(HttpMethod method, string path, JsonElement payload, object fallback) => SendCore(method, path, payload.GetRawText(), fallback);

    private async Task<IActionResult> ProxyDelete(string path, object fallback)
    {
        try
        {
            var response = await httpClientFactory.CreateClient("BarberSyncApi").DeleteAsync(BuildUrl(path));
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("AdminApi DELETE {Path} retornou {StatusCode}. Usando fallback demo.", path, response.StatusCode);
                return Ok(fallback);
            }
            return Content(await response.Content.ReadAsStringAsync(), "application/json", Encoding.UTF8);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha AdminApi DELETE {Path}. Usando fallback demo.", path);
            return Ok(fallback);
        }
    }

    private async Task<IActionResult> SendCore(HttpMethod method, string path, string json, object fallback)
    {
        try
        {
            using var request = new HttpRequestMessage(method, BuildUrl(path)) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
            var response = await httpClientFactory.CreateClient("BarberSyncApi").SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("AdminApi {Method} {Path} retornou {StatusCode}. Usando fallback demo.", method, path, response.StatusCode);
                return Ok(fallback);
            }
            return Content(await response.Content.ReadAsStringAsync(), "application/json", Encoding.UTF8);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha AdminApi {Method} {Path}. Usando fallback demo.", method, path);
            return Ok(fallback);
        }
    }

    private string BuildUrl(string path) => $"{(configuration["ApiSettings:BaseUrl"] ?? configuration["ApiBaseUrl"] ?? "http://localhost:8080").TrimEnd('/')}/{path.TrimStart('/')}";

    private static object DemoMutation(string message, JsonElement payload, string? id = null) => new { success = true, isDemo = true, message, id = id ?? $"demo-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", data = JsonSerializer.Deserialize<object>(payload.GetRawText()) };
    private static object DemoMutation(string message, string id) => new { success = true, isDemo = true, message, id };

    private static object DemoDashboard() => new
    {
        kpis = new object[] {
            new { label = "Receita hoje", value = "R$ 4.850", trend = "+12%" }, new { label = "Receita mês", value = "R$ 97.800", trend = "+18%" }, new { label = "Agendamentos hoje", value = "26", trend = "+6" },
            new { label = "Clientes ativos", value = "412", trend = "+31" }, new { label = "Atendimentos em andamento", value = "7", trend = "agora" }, new { label = "Comandas abertas", value = "9", trend = "R$ 780" },
            new { label = "Ticket médio", value = "R$ 83", trend = "+9%" }, new { label = "Estoque crítico", value = "4", trend = "repor" }, new { label = "Avaliação média", value = "4,8", trend = "⭐" },
            new { label = "Campanhas ativas", value = "3", trend = "CRM" }, new { label = "Totem online", value = "1", trend = "online" }, new { label = "Profissionais disponíveis", value = "5", trend = "escala" }
        },
        topServices = DemoServices(), featuredProfessionals = DemoProfessionals(), appointments = DemoAppointments(), serviceOrders = DemoServiceOrders(), stockCritical = DemoStock(), copilotSuggestions = DemoCopilotSuggestions(), isDemo = true
    };

    private static object[] DemoServices() => [
        new { id = "demo-corte", name = "Corte Masculino", category = "Barbearia", price = 45.00m, durationMinutes = 40, status = "ACTIVE", site = true, kiosk = true, mobile = true },
        new { id = "demo-barba", name = "Barba Tradicional", category = "Barbearia", price = 35.00m, durationMinutes = 30, status = "ACTIVE", site = true, kiosk = true, mobile = true },
        new { id = "demo-combo", name = "Corte + Barba", category = "Combo", price = 70.00m, durationMinutes = 60, status = "ACTIVE", site = true, kiosk = true, mobile = true },
        new { id = "demo-hidratacao", name = "Hidratação Premium", category = "Tratamento", price = 55.00m, durationMinutes = 45, status = "ACTIVE", site = true, kiosk = false, mobile = true }
    ];

    private static object[] DemoProfessionals() => [
        new { id = "pro-rafael", name = "Rafael Barber", specialty = "Fade e barba", status = "Disponível", rating = 4.9m },
        new { id = "pro-lucas", name = "Lucas Navalha", specialty = "Corte clássico", status = "Em atendimento", rating = 4.8m },
        new { id = "pro-camila", name = "Camila Beauty", specialty = "Coloração", status = "Disponível", rating = 4.9m },
        new { id = "pro-amanda", name = "Amanda Nails", specialty = "Nails premium", status = "Disponível", rating = 4.7m }
    ];

    private static object[] DemoClients() => Enumerable.Range(1, 10).Select(i => new { id = $"cli-{i}", name = $"Cliente Demo {i}", phone = $"(11) 9888{i}-000{i}", status = i % 3 == 0 ? "VIP" : "Ativo", visits = 3 + i, lastVisit = DateTime.Today.AddDays(-i).ToString("dd/MM/yyyy") }).Cast<object>().ToArray();
    private static object[] DemoAppointments() => Enumerable.Range(1, 8).Select(i => new { id = $"ag-{i}", client = $"Cliente Demo {i}", service = i % 2 == 0 ? "Corte + Barba" : "Corte Masculino", professional = i % 2 == 0 ? "Rafael Barber" : "Camila Beauty", time = $"{9 + i:00}:00", status = i % 3 == 0 ? "Em atendimento" : "Confirmado" }).Cast<object>().ToArray();
    private static object[] DemoServiceOrders() => Enumerable.Range(1, 5).Select(i => new { id = $"os-{i}", code = $"SO-10{i:00}", client = $"Cliente Demo {i}", status = i % 2 == 0 ? "Aberta" : "Em execução", total = 70 + i * 15 }).Cast<object>().ToArray();
    private static object[] DemoProducts() => Enumerable.Range(1, 10).Select(i => new { id = $"prod-{i}", name = $"Produto Demo {i}", category = "Estoque", stock = 5 + i, minimum = 8, status = (5 + i) <= 8 ? "Crítico" : "OK" }).Cast<object>().ToArray();
    private static object[] DemoStock() => [ new { id = "stk-1", name = "Lâmina Platinum", current = 6, minimum = 20, status = "Crítico" }, new { id = "stk-2", name = "Toalha Premium", current = 10, minimum = 25, status = "Atenção" }, new { id = "stk-3", name = "Pomada Modeladora", current = 7, minimum = 15, status = "Crítico" }, new { id = "stk-4", name = "Shampoo Anticaspa", current = 5, minimum = 10, status = "Crítico" } ];
    private static object[] DemoCampaigns() => [ new { id = "camp-1", name = "Volte e ganhe", channel = "WhatsApp", status = "Ativa", conversion = "18%" }, new { id = "camp-2", name = "Combo do mês", channel = "Instagram", status = "Ativa", conversion = "12%" }, new { id = "camp-3", name = "Aniversariantes", channel = "SMS", status = "Agendada", conversion = "--" } ];
    private static object[] DemoCoupons() => [ new { id = "cup-1", code = "BARBA10", discount = 10, status = "Ativo" }, new { id = "cup-2", code = "VIP15", discount = 15, status = "Ativo" }, new { id = "cup-3", code = "WELCOME5", discount = 5, status = "Ativo" } ];
    private static object[] DemoReviews() => [ new { id = "rev-1", client = "Marcos", rating = 5, comment = "Atendimento excelente", status = "Publicado" }, new { id = "rev-2", client = "Thiago", rating = 4, comment = "Ótimo acabamento", status = "Publicado" }, new { id = "rev-3", client = "Eduardo", rating = 5, comment = "Voltarei com certeza", status = "Publicado" } ];
    private static object DemoLoyalty() => new { name = "Clube BarberSync", status = "Ativo", pointsDistributed = 1800, activeMembers = 132, cashbackMonth = 2340, tiers = new[] { "Silver", "Gold", "Black" } };
    private static object[] DemoCopilotSuggestions() => [ new { title = "Retenção", description = "Reforçar campanha de retorno para clientes inativos há 45 dias.", priority = "Alta" }, new { title = "Escala", description = "Aumentar escala no horário de pico entre 18h e 20h.", priority = "Média" }, new { title = "Receita", description = "Criar cupom de recompra para serviços com baixa ocupação.", priority = "Alta" } ];
}
