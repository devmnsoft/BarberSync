using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
public class DemoOperationsController(ILogger<DemoOperationsController> logger) : ControllerBase
{
    private static readonly List<object> Products =
    [
        new { id = "prd-001", name = "Pomada Matte", category = "Finalização", stock = 12, minStock = 8, price = 39.90m, status = "OK" },
        new { id = "prd-002", name = "Shampoo Premium", category = "Tratamento", stock = 5, minStock = 10, price = 49.90m, status = "Crítico" },
        new { id = "prd-003", name = "Óleo para Barba", category = "Barba", stock = 7, minStock = 6, price = 59.90m, status = "OK" },
        new { id = "prd-004", name = "Lâminas Profissionais", category = "Insumo", stock = 3, minStock = 15, price = 24.90m, status = "Crítico" }
    ];

    private static readonly List<object> ServiceOrders =
    [
        new { id = "so-001", number = "CMD-1024", client = "Lucas Almeida", professional = "Rafael Barber", status = "Open", total = 110m, channel = "Admin", items = new[] { "Corte + Barba", "Pomada Matte" } },
        new { id = "so-002", number = "CMD-1025", client = "Marina Costa", professional = "Camila Beauty", status = "Paid", total = 145m, channel = "Totem", items = new[] { "Hidratação", "Finalização" } }
    ];

    private static readonly List<object> Leads =
    [
        new { id = "lead-001", protocol = "PUB-2026-0001", name = "Bruno Visitante", phone = "(11) 95555-0101", service = "Corte Masculino", status = "Novo", channel = "PublicWeb" }
    ];

    private static readonly List<object> KioskAttendances = [];
    private static readonly List<object> AuditEvents = [];

    [HttpGet("api/products")]
    public IActionResult GetProducts() => Ok(Envelope(Products, "Produtos carregados com sucesso."));

    [HttpGet("api/stock/critical")]
    public IActionResult GetCriticalStock() => Ok(Envelope(Products.Where(IsCritical).ToArray(), "Estoque crítico carregado com sucesso."));

    [HttpPost("api/stock/entry")]
    public IActionResult StockEntry([FromBody] JsonElement payload) => Ok(Mutation("Entrada de estoque registrada com sucesso.", payload));

    [HttpPost("api/stock/exit")]
    public IActionResult StockExit([FromBody] JsonElement payload) => Ok(Mutation("Saída de estoque registrada com sucesso.", payload));

    [HttpGet("api/service-orders")]
    public IActionResult GetServiceOrders() => Ok(Envelope(ServiceOrders, "Comandas carregadas com sucesso."));

    [HttpPost("api/service-orders/open")]
    public IActionResult OpenServiceOrder([FromBody] JsonElement payload)
    {
        var order = new { id = $"so-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", number = $"CMD-{Random.Shared.Next(2000, 9999)}", status = "Open", total = 0m, payload = ToObject(payload), channel = "Demo" };
        ServiceOrders.Insert(0, order);
        RegisterAudit("serviceOrder:opened", order);
        return Ok(Mutation("Comanda aberta com sucesso.", order));
    }

    [HttpPost("api/service-orders/{id}/add-service")]
    public IActionResult AddService(string id, [FromBody] JsonElement payload) => Ok(Mutation("Serviço adicionado à comanda.", payload, id));

    [HttpPost("api/service-orders/{id}/add-product")]
    public IActionResult AddProduct(string id, [FromBody] JsonElement payload) => Ok(Mutation("Produto adicionado à comanda e estoque reservado.", payload, id));

    [HttpPost("api/service-orders/{id}/pay")]
    public IActionResult PayServiceOrder(string id, [FromBody] JsonElement payload)
    {
        var payment = new { id = $"pay-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", serviceOrderId = id, status = "APPROVED", payload = ToObject(payload), paidAt = DateTime.UtcNow };
        RegisterAudit("payment:approved", payment);
        return Ok(Mutation("Pagamento aprovado e registrado com sucesso.", payment, id));
    }

    [HttpPost("api/service-orders/{id}/close")]
    public IActionResult CloseServiceOrder(string id, [FromBody] JsonElement payload) => Ok(Mutation("Comanda fechada com sucesso.", payload, id));

    [HttpGet("api/leads")]
    public IActionResult GetLeads() => Ok(Envelope(Leads, "Leads carregados com sucesso."));

    [HttpPost("api/leads")]
    public IActionResult CreateLead([FromBody] JsonElement payload)
    {
        var lead = new { id = $"lead-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", protocol = $"PUB-2026-{Leads.Count + 1:0000}", payload = ToObject(payload), status = "Novo", channel = "PublicWeb", createdAt = DateTime.UtcNow };
        Leads.Insert(0, lead);
        RegisterAudit("public:leadCreated", lead);
        return Ok(Mutation("Lead recebido e enviado ao Admin.", lead));
    }

    [HttpPost("api/kiosk/check-in")]
    public IActionResult KioskCheckIn([FromBody] JsonElement payload) => RegisterKiosk("Check-in realizado no totem.", payload, "CheckedIn");

    [HttpPost("api/kiosk/service-order")]
    public IActionResult KioskServiceOrder([FromBody] JsonElement payload) => RegisterKiosk("Comanda do totem criada.", payload, "ServiceOrderCreated");

    [HttpPost("api/kiosk/payment/mock")]
    public IActionResult KioskPayment([FromBody] JsonElement payload) => RegisterKiosk("Pagamento mock aprovado no totem.", payload, "PaymentApproved");

    [HttpPost("api/kiosk/review")]
    public IActionResult KioskReview([FromBody] JsonElement payload) => RegisterKiosk("Avaliação registrada no totem.", payload, "ReviewCreated");

    [HttpGet("api/kiosk/status")]
    public IActionResult KioskStatus() => Ok(Envelope(new { online = true, deviceCode = "KIOSK-DEMO-001", currentStep = "Pronto", attendances = KioskAttendances.Count, paymentsMockEnabled = true, lastHeartbeat = DateTime.UtcNow }, "Totem online."));

    [HttpGet("api/audit/events")]
    public IActionResult Audit() => Ok(Envelope(AuditEvents.Take(100).ToArray(), "Eventos de auditoria carregados."));

    [HttpPost("api/audit/events")]
    public IActionResult CreateAudit([FromBody] JsonElement payload)
    {
        RegisterAudit("client:audit", ToObject(payload));
        return Ok(Mutation("Evento de auditoria registrado.", payload));
    }

    [HttpGet("api/full-service-flow/snapshot")]
    public IActionResult FullServiceFlowSnapshot() => Ok(Envelope(new
    {
        steps = new[] { "Cliente", "Agendamento", "Check-in", "Atendimento", "Comanda", "Pagamento", "Recibo", "Estoque", "Cashback", "Avaliação", "Dashboard" },
        lastOrders = ServiceOrders.Take(5),
        kioskAttendances = KioskAttendances.Take(5),
        audit = AuditEvents.Take(10)
    }, "Fluxo completo sincronizado."));

    private IActionResult RegisterKiosk(string message, JsonElement payload, string status)
    {
        var attendance = new { id = $"kiosk-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", status, payload = ToObject(payload), createdAt = DateTime.UtcNow, channel = "Kiosk" };
        KioskAttendances.Insert(0, attendance);
        RegisterAudit($"kiosk:{status}", attendance);
        logger.LogInformation("{Message} {@Attendance}", message, attendance);
        return Ok(Mutation(message, attendance));
    }

    private static object Envelope(object data, string message) => new { success = true, message, data, items = data, isDemo = true, traceId = Guid.NewGuid().ToString("N") };

    private static object Mutation(string message, object data, string? id = null) => new { success = true, message, data, id, isDemo = true, traceId = Guid.NewGuid().ToString("N") };

    private static object? ToObject(JsonElement payload) => JsonSerializer.Deserialize<object>(payload.GetRawText());

    private static bool IsCritical(object product)
    {
        var type = product.GetType();
        var stock = Convert.ToDecimal(type.GetProperty("stock")?.GetValue(product) ?? type.GetProperty("Stock")?.GetValue(product) ?? 0);
        var min = Convert.ToDecimal(type.GetProperty("minStock")?.GetValue(product) ?? type.GetProperty("MinStock")?.GetValue(product) ?? 0);
        return stock <= min;
    }

    private static void RegisterAudit(string eventName, object payload)
    {
        AuditEvents.Insert(0, new { id = $"aud-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}", eventName, payload, createdAt = DateTime.UtcNow, severity = "info" });
    }
}
