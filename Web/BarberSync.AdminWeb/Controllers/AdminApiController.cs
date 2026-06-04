using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

[ApiController]
[Route("AdminApi")]
public class AdminApiController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AdminApiController> logger) : ControllerBase
{
    [HttpGet("dashboard")] public Task<IActionResult> Dashboard() => ProxyGet("/api/dashboard/summary", DemoDashboard(), "Dashboard carregado em modo demonstração.");
    [HttpGet("clients")] public Task<IActionResult> Clients() => ProxyGet("/api/clients", DemoClients(), "Clientes carregados em modo demonstração.");
    [HttpGet("professionals")] public Task<IActionResult> Professionals() => ProxyGet("/api/professionals", DemoProfessionals(), "Profissionais carregados em modo demonstração.");
    [HttpGet("services")] public Task<IActionResult> Services() => ProxyGet("/api/services", DemoServices(), "Serviços carregados em modo demonstração.");
    [HttpGet("appointments")] public Task<IActionResult> Appointments() => ProxyGet("/api/appointments", DemoAppointments(), "Agenda carregada em modo demonstração.");
    [HttpGet("service-orders")] public Task<IActionResult> ServiceOrders() => ProxyGet("/api/service-orders", DemoServiceOrders(), "Comandas carregadas em modo demonstração.");
    [HttpGet("products")] public Task<IActionResult> Products() => ProxyGet("/api/products", DemoProducts(), "Produtos carregados em modo demonstração.");
    [HttpGet("stock-critical")] public Task<IActionResult> StockCritical() => ProxyGet("/api/stock/critical", DemoStockCritical(), "Estoque crítico carregado em modo demonstração.");
    [HttpGet("campaigns")] public Task<IActionResult> Campaigns() => ProxyGet("/api/campaigns", DemoCampaigns(), "Campanhas carregadas em modo demonstração.");
    [HttpGet("coupons")] public Task<IActionResult> Coupons() => ProxyGet("/api/coupons", DemoCoupons(), "Cupons carregados em modo demonstração.");
    [HttpGet("reviews")] public Task<IActionResult> Reviews() => ProxyGet("/api/reviews", DemoReviews(), "Avaliações carregadas em modo demonstração.");
    [HttpGet("loyalty")] public Task<IActionResult> Loyalty() => ProxyGet("/api/loyalty/summary", DemoLoyalty(), "Fidelidade carregada em modo demonstração.");
    [HttpGet("copilot-suggestions")] public Task<IActionResult> CopilotSuggestions() => ProxyGet("/api/copilot/suggestions", DemoCopilotSuggestions(), "Sugestões Copilot carregadas em modo demonstração.");
    [HttpGet("kiosk-status")] public Task<IActionResult> KioskStatus() => ProxyGet("/api/kiosk/status", DemoKioskStatus(), "Status do totem carregado em modo demonstração.");
    [HttpGet("financial-summary")] public Task<IActionResult> FinancialSummary() => ProxyGet("/api/financial/summary", DemoFinancialSummary(), "Resumo financeiro carregado em modo demonstração.");
    [HttpGet("reports-summary")] public Task<IActionResult> ReportsSummary() => ProxyGet("/api/reports/summary", DemoReportsSummary(), "Resumo de relatórios carregado em modo demonstração.");

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
    [HttpPost("appointments/{id}/confirm")] public Task<IActionResult> ConfirmAppointment(string id) => ProxySend(HttpMethod.Post, $"/api/appointments/{Uri.EscapeDataString(id)}/confirm", null, DemoMutation("Agendamento confirmado em modo demonstração.", id));
    [HttpPost("appointments/{id}/check-in")] public Task<IActionResult> CheckInAppointment(string id) => ProxySend(HttpMethod.Post, $"/api/appointments/{Uri.EscapeDataString(id)}/check-in", null, DemoMutation("Check-in realizado em modo demonstração.", id));
    [HttpPost("appointments/{id}/start")] public Task<IActionResult> StartAppointment(string id) => ProxySend(HttpMethod.Post, $"/api/appointments/{Uri.EscapeDataString(id)}/start", null, DemoMutation("Atendimento iniciado em modo demonstração.", id));
    [HttpPost("appointments/{id}/finish")] public Task<IActionResult> FinishAppointment(string id) => ProxySend(HttpMethod.Post, $"/api/appointments/{Uri.EscapeDataString(id)}/finish", null, DemoMutation("Atendimento finalizado em modo demonstração.", id));
    [HttpPost("appointments/{id}/cancel")] public Task<IActionResult> CancelAppointment(string id) => ProxySend(HttpMethod.Post, $"/api/appointments/{Uri.EscapeDataString(id)}/cancel", null, DemoMutation("Agendamento cancelado em modo demonstração.", id));

    [HttpPost("service-orders/open")] public Task<IActionResult> OpenServiceOrder([FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, "/api/service-orders/open", payload, DemoMutation("Comanda aberta em modo demonstração.", payload));
    [HttpPost("service-orders/{id}/add-service")] public Task<IActionResult> AddServiceToOrder(string id, [FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, $"/api/service-orders/{Uri.EscapeDataString(id)}/add-service", payload, DemoMutation("Serviço adicionado à comanda em modo demonstração.", payload, id));
    [HttpPost("service-orders/{id}/add-product")] public Task<IActionResult> AddProductToOrder(string id, [FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, $"/api/service-orders/{Uri.EscapeDataString(id)}/add-product", payload, DemoMutation("Produto adicionado à comanda em modo demonstração.", payload, id));
    [HttpPost("service-orders/{id}/pay")] public Task<IActionResult> PayServiceOrder(string id, [FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, $"/api/service-orders/{Uri.EscapeDataString(id)}/pay", payload, DemoMutation("Pagamento da comanda aprovado em modo demonstração.", payload, id));
    [HttpPost("service-orders/{id}/close")] public Task<IActionResult> CloseServiceOrder(string id, [FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, $"/api/service-orders/{Uri.EscapeDataString(id)}/close", payload, DemoMutation("Comanda fechada em modo demonstração.", payload, id));

    [HttpPost("stock/entry")] public Task<IActionResult> StockEntry([FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, "/api/stock/entry", payload, DemoMutation("Entrada de estoque registrada em modo demonstração.", payload));
    [HttpPost("stock/exit")] public Task<IActionResult> StockExit([FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, "/api/stock/exit", payload, DemoMutation("Saída de estoque registrada em modo demonstração.", payload));
    [HttpPost("campaigns")] public Task<IActionResult> CreateCampaign([FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, "/api/campaigns", payload, DemoMutation("Campanha criada em modo demonstração.", payload));
    [HttpPost("coupons")] public Task<IActionResult> CreateCoupon([FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, "/api/coupons", payload, DemoMutation("Cupom criado em modo demonstração.", payload));
    [HttpPost("copilot/ask")] public Task<IActionResult> AskCopilot([FromBody] JsonElement payload) => ProxySend(HttpMethod.Post, "/api/copilot/ask", payload, new { success = true, message = "Copilot respondeu em modo demonstração.", data = new { answer = "Sugestão: priorize clientes VIP sem visita nos últimos 30 dias e ofereça combo Corte + Barba com cashback.", actions = new[] { "Criar campanha WhatsApp", "Gerar cupom VIP15", "Reforçar escala 18h-20h" }, isDemo = true } });

    private async Task<IActionResult> ProxyGet(string path, object fallbackData, string fallbackMessage)
    {
        try
        {
            var response = await httpClientFactory.CreateClient().GetAsync(BuildUrl(path));
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                if (!ResponseLooksEmpty(json)) return Content(json, "application/json", Encoding.UTF8);
                logger.LogWarning("AdminApi GET {Path} retornou payload vazio. Usando fallback demo.", path);
            }
            else
            {
                logger.LogWarning("AdminApi GET {Path} retornou {StatusCode}. Usando fallback demo.", path, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "AdminApi GET {Path} lançou exceção. Usando fallback demo.", path);
        }

        return Ok(new { success = true, message = fallbackMessage, data = fallbackData, isDemo = true });
    }

    private async Task<IActionResult> ProxySend(HttpMethod method, string path, JsonElement? payload, object fallback)
    {
        try
        {
            using var request = new HttpRequestMessage(method, BuildUrl(path));
            if (payload.HasValue) request.Content = new StringContent(payload.Value.GetRawText(), Encoding.UTF8, "application/json");
            var response = await httpClientFactory.CreateClient().SendAsync(request);
            if (response.IsSuccessStatusCode) return Content(await response.Content.ReadAsStringAsync(), "application/json", Encoding.UTF8);
            logger.LogWarning("AdminApi {Method} {Path} retornou {StatusCode}. Usando fallback demo.", method, path, response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "AdminApi {Method} {Path} lançou exceção. Usando fallback demo.", method, path);
        }

        return Ok(fallback);
    }

    private async Task<IActionResult> ProxyDelete(string path, object fallback)
    {
        try
        {
            var response = await httpClientFactory.CreateClient().DeleteAsync(BuildUrl(path));
            if (response.IsSuccessStatusCode) return Content(await response.Content.ReadAsStringAsync(), "application/json", Encoding.UTF8);
            logger.LogWarning("AdminApi DELETE {Path} retornou {StatusCode}. Usando fallback demo.", path, response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "AdminApi DELETE {Path} lançou exceção. Usando fallback demo.", path);
        }

        return Ok(fallback);
    }

    private string BuildUrl(string path) => $"{(configuration["ApiSettings:BaseUrl"] ?? configuration["ApiBaseUrl"] ?? "http://localhost:8080").TrimEnd('/')}/{path.TrimStart('/')}";

    private static bool ResponseLooksEmpty(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return true;
        try
        {
            using var doc = JsonDocument.Parse(json);
            return ElementLooksEmpty(doc.RootElement);
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
    private static object DemoMutation(string message, JsonElement payload, string? id = null) => new { success = true, message, data = new { id = id ?? $"demo-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", payload = JsonSerializer.Deserialize<object>(payload.GetRawText()), isDemo = true } };
    private static object DemoMutation(string message, string id) => new { success = true, message, data = new { id, isDemo = true } };

    private static object DemoDashboard() => new
    {
        kpis = new object[] { new { label = "Receita hoje", value = "R$ 4.850", trend = "+12%" }, new { label = "Receita mês", value = "R$ 97.800", trend = "+18%" }, new { label = "Agendamentos hoje", value = "26", trend = "+6" }, new { label = "Clientes ativos", value = "412", trend = "+31" }, new { label = "Atendimentos em andamento", value = "7", trend = "agora" }, new { label = "Comandas abertas", value = "9", trend = "R$ 780" }, new { label = "Ticket médio", value = "R$ 83", trend = "+9%" }, new { label = "Estoque crítico", value = "4", trend = "repor" }, new { label = "Avaliação média", value = "4,8", trend = "⭐" }, new { label = "Campanhas ativas", value = "3", trend = "CRM" }, new { label = "Totem online", value = "1", trend = "online" }, new { label = "Profissionais disponíveis", value = "5", trend = "escala" } },
        revenueLast7Days = new object[] { new { day = "Seg", total = 3820 }, new { day = "Ter", total = 4210 }, new { day = "Qua", total = 4680 }, new { day = "Qui", total = 5120 }, new { day = "Sex", total = 5840 }, new { day = "Sáb", total = 7350 }, new { day = "Dom", total = 4850 } },
        appointmentsByStatus = new object[] { new { status = "Confirmado", total = 14 }, new { status = "Check-in", total = 5 }, new { status = "Em atendimento", total = 7 }, new { status = "Finalizado", total = 18 } },
        topServices = DemoServices().Take(6).ToArray(), paymentMethods = new object[] { new { method = "PIX", total = 48 }, new { method = "Cartão", total = 37 }, new { method = "Dinheiro", total = 15 } },
        professionalOccupancy = DemoProfessionals().Take(6).ToArray(), upcomingAppointments = DemoAppointments().Take(6).ToArray(), appointments = DemoAppointments().Take(6).ToArray(),
        openServiceOrders = DemoServiceOrders().Where(x => !JsonSerializer.Serialize(x).Contains("Fechada")).ToArray(), serviceOrders = DemoServiceOrders().Take(5).ToArray(), criticalStock = DemoStockCritical(), stockCritical = DemoStockCritical(), copilotSuggestions = DemoCopilotSuggestions(), alerts = new object[] { new { type = "stock", message = "4 itens abaixo do estoque mínimo." }, new { type = "schedule", message = "Pico de demanda previsto entre 18h e 20h." }, new { type = "crm", message = "32 clientes VIP sem retorno em 30 dias." } }, isDemo = true
    };

    private static object[] DemoClients()
    {
        var names = new[]
        {
            "Marcos Vinícius", "Thiago Almeida", "Fernanda Costa", "Eduardo Lima", "Barber Prime Ltda",
            "Renata Martins", "Lucas Pereira", "Camila Rocha", "André Souza", "Patrícia Nunes",
            "Gustavo Reis", "Juliana Santos", "Bruno Carvalho", "Tatiane Moura", "Felipe Andrade"
        };
        var cities = new[] { "São Paulo", "Santo André", "Osasco", "Guarulhos", "Campinas" };
        var services = new[] { "Corte Masculino", "Combo Corte + Barba", "Barba Tradicional", "Hidratação Premium", "Sobrancelha" };
        var professionals = new[] { "Rafael Barber", "Lucas Navalha", "Camila Beauty", "Amanda Nails", "Bianca Studio" };
        return names.Select((name, i) => new
        {
            id = $"cli-{i + 1:000}",
            name,
            type = name.Contains("Ltda", StringComparison.OrdinalIgnoreCase) ? "PJ" : "PF",
            document = name.Contains("Ltda", StringComparison.OrdinalIgnoreCase) ? "12.345.678/0001-90" : $"{123 + i:000}.456.{789 - i:000}-{i + 1:00}",
            phone = $"(11) 988{i + 1:00}-10{i + 1:00}",
            whatsapp = $"(11) 988{i + 1:00}-10{i + 1:00}",
            email = $"cliente{i + 1:00}@barbersync.demo",
            lastVisit = DateTime.Today.AddDays(-(i + 2)).ToString("dd/MM/yyyy"),
            cashback = Math.Round(8.5m + (i * 7.35m), 2),
            isVip = i % 4 == 0 || i == 2,
            status = i == 10 ? "Inativo" : (i % 4 == 0 || i == 2 ? "VIP" : "Ativo"),
            city = cities[i % cities.Length],
            preferredService = services[i % services.Length],
            preferredProfessional = professionals[i % professionals.Length],
            totalSpent = 280m + (i * 145m),
            averageTicket = 62m + (i % 5 * 9m),
            nextAppointment = DateTime.Today.AddDays(i % 6).AddHours(10 + (i % 8)).ToString("dd/MM/yyyy HH:mm")
        }).Cast<object>().ToArray();
    }

    private static object[] DemoProfessionals() => new object[]
    {
        new { id="pro-001", name="Rafael Barber", specialty="Fade e barba", phone="(11) 97701-1001", email="rafael@barbersync.demo", status="Disponível", rating=4.9m, monthlyRevenue=28400m, appointmentsToday=8, commissionPercent=40, services=new[] { "Corte Masculino", "Barba Tradicional", "Combo Corte + Barba" }, occupancy=88, ranking=1, monthlyGoal=32000m },
        new { id="pro-002", name="Lucas Navalha", specialty="Corte clássico", phone="(11) 97702-1002", email="lucas@barbersync.demo", status="Em atendimento", rating=4.8m, monthlyRevenue=24200m, appointmentsToday=6, commissionPercent=38, services=new[] { "Corte Masculino", "Sobrancelha" }, occupancy=79, ranking=3, monthlyGoal=28000m },
        new { id="pro-003", name="Camila Beauty", specialty="Visagismo e coloração", phone="(11) 97703-1003", email="camila@barbersync.demo", status="Disponível", rating=4.9m, monthlyRevenue=31800m, appointmentsToday=7, commissionPercent=42, services=new[] { "Coloração", "Hidratação Premium" }, occupancy=91, ranking=2, monthlyGoal=35000m },
        new { id="pro-004", name="Amanda Nails", specialty="Nails premium", phone="(11) 97704-1004", email="amanda@barbersync.demo", status="Disponível", rating=4.7m, monthlyRevenue=19800m, appointmentsToday=5, commissionPercent=40, services=new[] { "Manicure", "Pedicure" }, occupancy=72, ranking=6, monthlyGoal=24000m },
        new { id="pro-005", name="Diego Skin", specialty="Estética masculina", phone="(11) 97705-1005", email="diego@barbersync.demo", status="Folga", rating=4.8m, monthlyRevenue=17600m, appointmentsToday=0, commissionPercent=35, services=new[] { "Limpeza de pele", "Massagem capilar" }, occupancy=64, ranking=8, monthlyGoal=22000m },
        new { id="pro-006", name="Bianca Studio", specialty="Design de sobrancelhas", phone="(11) 97706-1006", email="bianca@barbersync.demo", status="Disponível", rating=4.9m, monthlyRevenue=22100m, appointmentsToday=4, commissionPercent=37, services=new[] { "Sobrancelha", "Micropigmentação" }, occupancy=83, ranking=4, monthlyGoal=26000m },
        new { id="pro-007", name="Nicolas Prime", specialty="Barba premium", phone="(11) 97707-1007", email="nicolas@barbersync.demo", status="Disponível", rating=4.8m, monthlyRevenue=20500m, appointmentsToday=5, commissionPercent=39, services=new[] { "Barba Tradicional", "Combo Corte + Barba" }, occupancy=76, ranking=5, monthlyGoal=25000m },
        new { id="pro-008", name="Larissa Spa", specialty="Tratamentos e bem-estar", phone="(11) 97708-1008", email="larissa@barbersync.demo", status="Disponível", rating=4.9m, monthlyRevenue=18400m, appointmentsToday=3, commissionPercent=36, services=new[] { "Hidratação Premium", "Massagem Capilar" }, occupancy=68, ranking=7, monthlyGoal=23000m }
    };

    private static object[] DemoServices() => new object[]
    {
        new { id="srv-001", name="Corte Masculino", category="Barbearia", description="Corte moderno com consultoria de estilo e acabamento.", price=45m, durationMinutes=40, commissionPercent=40, visibleOnPublicWeb=true, visibleOnKiosk=true, visibleOnMobile=true, status="Ativo" },
        new { id="srv-002", name="Barba Tradicional", category="Barbearia", description="Toalha quente, navalha e balm premium.", price=35m, durationMinutes=30, commissionPercent=38, visibleOnPublicWeb=true, visibleOnKiosk=true, visibleOnMobile=true, status="Ativo" },
        new { id="srv-003", name="Combo Corte + Barba", category="Combo", description="Experiência completa BarberSync.", price=70m, durationMinutes=60, commissionPercent=40, visibleOnPublicWeb=true, visibleOnKiosk=true, visibleOnMobile=true, status="Ativo" },
        new { id="srv-004", name="Sobrancelha", category="Estética", description="Design e limpeza com acabamento natural.", price=25m, durationMinutes=20, commissionPercent=35, visibleOnPublicWeb=true, visibleOnKiosk=true, visibleOnMobile=true, status="Ativo" },
        new { id="srv-005", name="Hidratação Premium", category="Tratamento", description="Tratamento capilar com produtos profissionais.", price=55m, durationMinutes=45, commissionPercent=36, visibleOnPublicWeb=true, visibleOnKiosk=false, visibleOnMobile=true, status="Ativo" },
        new { id="srv-006", name="Coloração", category="Coloração", description="Coloração técnica com diagnóstico prévio.", price=120m, durationMinutes=90, commissionPercent=42, visibleOnPublicWeb=true, visibleOnKiosk=false, visibleOnMobile=true, status="Ativo" },
        new { id="srv-007", name="Manicure", category="Nails", description="Cutilagem, esmaltação e hidratação.", price=40m, durationMinutes=45, commissionPercent=40, visibleOnPublicWeb=true, visibleOnKiosk=true, visibleOnMobile=true, status="Ativo" },
        new { id="srv-008", name="Pedicure", category="Nails", description="Cuidado completo para os pés.", price=48m, durationMinutes=50, commissionPercent=40, visibleOnPublicWeb=true, visibleOnKiosk=true, visibleOnMobile=true, status="Ativo" },
        new { id="srv-009", name="Limpeza de Pele", category="Estética", description="Higienização facial e máscara calmante.", price=95m, durationMinutes=70, commissionPercent=35, visibleOnPublicWeb=true, visibleOnKiosk=false, visibleOnMobile=true, status="Ativo" },
        new { id="srv-010", name="Massagem Capilar", category="Bem-estar", description="Relaxamento com óleos essenciais.", price=65m, durationMinutes=35, commissionPercent=35, visibleOnPublicWeb=true, visibleOnKiosk=true, visibleOnMobile=true, status="Ativo" },
        new { id="srv-011", name="Micropigmentação", category="Estética", description="Procedimento premium com avaliação prévia.", price=180m, durationMinutes=120, commissionPercent=45, visibleOnPublicWeb=true, visibleOnKiosk=false, visibleOnMobile=true, status="Ativo" },
        new { id="srv-012", name="Dia do Noivo", category="Experiência", description="Pacote completo com corte, barba, spa e bebida de boas-vindas.", price=220m, durationMinutes=150, commissionPercent=42, visibleOnPublicWeb=true, visibleOnKiosk=true, visibleOnMobile=true, status="Ativo" }
    };

    private static object[] DemoAppointments()
    {
        var statuses = new[] { "Agendado", "Confirmado", "Check-in", "Em atendimento", "Finalizado", "Cancelado" };
        return DemoClients().Take(15).Select((client, i) => new
        {
            id = $"apt-{i + 1:000}",
            clientName = ReadProp(client, "name"),
            client = ReadProp(client, "name"),
            professionalName = ReadProp(DemoProfessionals()[i % 8], "name"),
            professional = ReadProp(DemoProfessionals()[i % 8], "name"),
            serviceName = ReadProp(DemoServices()[i % 12], "name"),
            service = ReadProp(DemoServices()[i % 12], "name"),
            scheduledAt = DateTime.Today.AddHours(8 + (i % 10)).AddMinutes(i % 2 == 0 ? 0 : 30).ToString("yyyy-MM-ddTHH:mm:ss"),
            time = $"{8 + (i % 10):00}:{(i % 2 == 0 ? "00" : "30")}",
            price = 45m + (i % 5 * 15m),
            status = statuses[i % statuses.Length],
            notes = "Preferência registrada no CRM demo. Confirmar WhatsApp antes do horário."
        }).Cast<object>().ToArray();
    }

    private static readonly string[] DemoNames = new[] { "Marcos Vinícius", "Thiago Almeida", "Fernanda Costa", "Eduardo Lima", "Barber Prime Ltda", "Renata Martins", "Lucas Pereira", "Camila Rocha", "André Souza", "Patrícia Nunes", "Gustavo Reis", "Juliana Santos", "Bruno Carvalho", "Tatiane Moura", "Felipe Andrade" };

    private static object[] DemoServiceOrders() => Enumerable.Range(1, 10).Select(i => new
    {
        id=$"so-{i:000}",
        number=$"SO-2026-{1000 + i}",
        code=$"SO-2026-{1000 + i}",
        clientName=DemoNames[i - 1],
        client=DemoNames[i - 1],
        professionalName=i % 2 == 0 ? "Rafael Barber" : "Camila Beauty",
        items=new object[] { new { name="Corte Masculino", quantity=1, total=45m }, new { name=i % 2 == 0 ? "Pomada Modeladora" : "Barba Tradicional", quantity=1, total=i % 2 == 0 ? 39m : 35m } },
        subtotal=i % 2 == 0 ? 84m : 80m,
        discount=i % 3 == 0 ? 10m : 0m,
        cashback=i % 4 == 0 ? 8m : 0m,
        total=(i % 2 == 0 ? 84m : 80m) - (i % 3 == 0 ? 10m : 0m),
        paidAmount=i > 6 ? ((i % 2 == 0 ? 84m : 80m) - (i % 3 == 0 ? 10m : 0m)) : 0m,
        payments=i > 6 ? new object[] { new { method = i % 2 == 0 ? "PIX" : "Cartão", amount = (i % 2 == 0 ? 84m : 80m) - (i % 3 == 0 ? 10m : 0m) } } : Array.Empty<object>(),
        status=i > 6 ? "Fechada" : i % 3 == 0 ? "Pagamento" : "Aberta"
    }).Cast<object>().ToArray();

    private static object[] DemoProducts()
    {
        var names = new[] { "Lâmina Platinum", "Pomada Modeladora", "Shampoo Anticaspa", "Balm para Barba", "Toalha Premium", "Óleo Essencial", "Gel Cola", "Máscara Facial", "Esmalte Nude", "Creme Hidratante", "Capa de Corte", "Luvas Nitrílicas", "Desinfetante Hospitalar", "Cera Modeladora", "Pós-Barba Ice" };
        return names.Select((name, i) => new
        {
            id=$"prod-{i + 1:000}",
            name,
            category=i % 3 == 0 ? "Tratamento" : i % 2 == 0 ? "Revenda" : "Insumo",
            sku=$"BS-{i + 1:0000}",
            costPrice=8m + i,
            salePrice=18m + ((i + 1) * 3),
            currentStock=i < 5 ? 4 + i : 15 + i,
            minStock=i < 5 ? 12 : 10,
            stock=i < 5 ? 4 + i : 15 + i,
            minimum=i < 5 ? 12 : 10,
            status=i < 3 ? "Crítico" : i < 5 ? "Atenção" : "Normal",
            purchaseSuggestion = i < 5 ? $"Comprar {20 - i} unidades antes do fim de semana." : "Estoque saudável."
        }).Cast<object>().ToArray();
    }

    private static object[] DemoStockCritical() => DemoProducts().Take(5).ToArray();

    private static object[] DemoCampaigns() => new object[]
    {
        new { id="camp-001", name="Volte e Ganhe", channel="WhatsApp", audience="Clientes 45 dias sem visita", period="01/06 a 15/06", status="Ativa", conversion="18%", result="R$ 6.200 previstos" },
        new { id="camp-002", name="Combo do Mês", channel="Instagram", audience="Novos clientes", period="Maio/2026", status="Ativa", conversion="12%", result="86 agendamentos" },
        new { id="camp-003", name="Aniversariantes", channel="SMS", audience="Clientes do mês", period="Mensal", status="Agendada", conversion="--", result="Disparo amanhã" },
        new { id="camp-004", name="VIP Black", channel="E-mail", audience="VIP", period="Última semana", status="Ativa", conversion="24%", result="Ticket +22%" },
        new { id="camp-005", name="Indique um Amigo", channel="WhatsApp", audience="Base ativa", period="Trimestral", status="Pausada", conversion="9%", result="31 indicações" },
        new { id="camp-006", name="Pós-avaliação 5 estrelas", channel="WhatsApp", audience="Promotores NPS", period="Automática", status="Ativa", conversion="15%", result="Cupom compartilhado" }
    };

    private static object[] DemoCoupons() => new object[]
    {
        new { id="cup-001", code="BARBA10", discount="10%", validUntil="2026-06-30", usage="42/200", status="Ativo" },
        new { id="cup-002", code="VIP15", discount="15%", validUntil="2026-06-15", usage="18/80", status="Ativo" },
        new { id="cup-003", code="WELCOME5", discount="R$ 5", validUntil="2026-07-01", usage="65/300", status="Ativo" },
        new { id="cup-004", code="COMBO20", discount="20%", validUntil="2026-06-10", usage="11/50", status="Pausado" },
        new { id="cup-005", code="NIVER25", discount="25%", validUntil="2026-12-31", usage="7/120", status="Ativo" },
        new { id="cup-006", code="CASHBACKDOBRO", discount="2x cashback", validUntil="2026-06-20", usage="29/150", status="Ativo" },
        new { id="cup-007", code="TOTEMFAST", discount="R$ 8", validUntil="2026-06-25", usage="14/100", status="Ativo" },
        new { id="cup-008", code="NOIVO30", discount="30%", validUntil="2026-09-30", usage="3/30", status="Ativo" }
    };

    private static object[] DemoReviews() => Enumerable.Range(1, 12).Select(i => new
    {
        id=$"rev-{i:000}",
        client=DemoNames[i - 1],
        rating=i % 6 == 0 ? 3 : i % 4 == 0 ? 4 : 5,
        nps=i % 6 == 0 ? 6 : i % 4 == 0 ? 8 : 10,
        comment=i % 6 == 0 ? "Tempo de espera poderia ser menor." : i % 2 == 0 ? "Atendimento pontual e acabamento excelente." : "Experiência premium, recomendo.",
        professional=i % 2 == 0 ? "Rafael Barber" : "Camila Beauty",
        type=i % 6 == 0 ? "Detrator" : i % 4 == 0 ? "Neutro" : "Promotor",
        recoveryAction=i % 6 == 0 ? "Enviar pedido de desculpas e cupom de retorno." : "Solicitar indicação nas redes sociais.",
        status="Publicado"
    }).Cast<object>().ToArray();

    private static object DemoLoyalty() => new
    {
        totalCashback=18420m,
        clientsWithCashback=132,
        expiringCashback=2140m,
        balance=18420m,
        cashbackMonth=2340m,
        activeMembers=132,
        customersWithCashback=DemoClients().Take(8).ToArray(),
        statement=new object[] { new { date="2026-05-28", client="Marcos Vinícius", amount=12.5m, type="Crédito" }, new { date="2026-05-27", client="Fernanda Costa", amount=20m, type="Resgate" }, new { date="2026-05-26", client="Juliana Santos", amount=8m, type="Crédito" }, new { date="2026-05-25", client="Tatiane Moura", amount=14m, type="Expira em 7 dias" } },
        tiers=new[] { "Silver", "Gold", "Black" },
        status="Ativo"
    };

    private static object[] DemoCopilotSuggestions() => new object[]
    {
        new { title="Retenção VIP", description="Acionar clientes VIP sem visita nos últimos 30 dias com cupom VIP15.", priority="Alta", actionLabel="Criar campanha", impact="R$ 6.200" },
        new { title="Escala inteligente", description="Reforçar profissionais entre 18h e 20h para reduzir espera no totem.", priority="Alta", actionLabel="Abrir agenda", impact="+14 atendimentos" },
        new { title="Estoque crítico", description="Comprar lâminas, toalhas e pomadas antes do pico do fim de semana.", priority="Média", actionLabel="Ver estoque", impact="Evita ruptura" },
        new { title="Upsell de combos", description="Oferecer Combo Corte + Barba aos clientes de corte avulso.", priority="Média", actionLabel="Criar cupom", impact="+R$ 18 ticket" },
        new { title="Reputação", description="Solicitar avaliações após pagamentos PIX para ampliar prova social.", priority="Baixa", actionLabel="Ver avaliações", impact="+0,2 rating" },
        new { title="Cashback inteligente", description="Dobrar cashback para clientes Black sem visita desde maio.", priority="Média", actionLabel="Ativar cashback", impact="+22 retornos" }
    };

    private static string ReadProp(object value, string property)
        => value.GetType().GetProperty(property)?.GetValue(value)?.ToString() ?? string.Empty;

    private static object DemoKioskStatus() => new { online = true, deviceCode = "KIOSK-DEMO-001", currentStep = "Pronto para atendimento", queue = 3, lastHeartbeat = DateTime.UtcNow.ToString("o"), paymentsMockEnabled = true, status = "Online" };
    private static object DemoFinancialSummary() => new { revenueToday = 4280m, revenueMonth = 86420m, openOrders = 6, paidOrders = 38, averageTicket = 92.5m, paymentSplit = new[] { new { method = "PIX", amount = 35200m, percent = 41 }, new { method = "Cartão", amount = 29400m, percent = 34 }, new { method = "Dinheiro", amount = 11200m, percent = 13 }, new { method = "Carteira", amount = 10620m, percent = 12 } }, status = "Demo" };
    private static object DemoReportsSummary() => new { generatedToday = 7, scheduled = 4, highlights = new[] { "Receita 18% acima da semana anterior", "No-show reduzido para 4%", "Campanha VIP15 com 24% de conversão" }, exports = new[] { "DRE gerencial", "Ocupação por profissional", "Estoque crítico", "NPS e avaliações" }, status = "Demo" };
}
