using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api")]
public class CommercialOpsController : ControllerBase
{
    [HttpGet("service-orders")]
    public IActionResult GetServiceOrders()
        => Ok(new[]
        {
            new { id = Guid.NewGuid(), number = "CMD-2026-001", client = "João Silva", professional = "Marina", items = 3, total = 145.00m, status = "Open" },
            new { id = Guid.NewGuid(), number = "CMD-2026-002", client = "Carlos Lima", professional = "Rafael", items = 2, total = 95.00m, status = "Paid" }
        });

    [HttpGet("products")]
    public IActionResult GetProducts()
        => Ok(new[]
        {
            new { id = Guid.NewGuid(), name = "Pomada Modeladora", category = "Finalização", stock = 8, minStock = 10, salePrice = 49.90m, status = "Critical" },
            new { id = Guid.NewGuid(), name = "Shampoo Anticaspa", category = "Higiene", stock = 24, minStock = 8, salePrice = 39.90m, status = "Active" }
        });

    [HttpGet("stock/critical")]
    public IActionResult GetCriticalStock()
        => Ok(new[]
        {
            new { id = Guid.NewGuid(), product = "Pomada Modeladora", current = 8, minimum = 10, gap = 2 },
            new { id = Guid.NewGuid(), product = "Navalha Profissional", current = 4, minimum = 12, gap = 8 }
        });

    [HttpPost("leads")]
    public IActionResult CreateLead([FromBody] JsonElement payload)
        => Ok(new { success = true, message = "Lead recebido com sucesso.", data = new { id = Guid.NewGuid(), protocol = $"LEAD-{DateTime.UtcNow:yyyyMMddHHmmss}", payload } });

    [HttpGet("loyalty/summary")]
    public IActionResult LoyaltySummary()
        => Ok(new { success = true, data = new { cashbackBalance = 1240.50m, pointsIssued = 42800, activeMembers = 318, redemptionRate = 27, tier = "Gold Demo" } });

    [HttpGet("copilot/suggestions")]
    public IActionResult CopilotSuggestions()
        => Ok(new[]
        {
            new { id = "cop-001", title = "Recuperar clientes inativos", priority = "Alta", impact = "R$ 4.200", action = "Criar campanha WhatsApp" },
            new { id = "cop-002", title = "Repor Pomada Modeladora", priority = "Crítica", impact = "Evita ruptura", action = "Gerar pedido ao fornecedor" },
            new { id = "cop-003", title = "Abrir agenda extra sexta", priority = "Média", impact = "+18% capacidade", action = "Convidar profissional parceiro" }
        });

    [HttpGet("financial/summary")]
    public IActionResult FinancialSummary()
        => Ok(new { revenueToday = 1840.00m, monthRevenue = 28400.00m, cash = 520.00m, pix = 960.00m, card = 360.00m, pending = 240.00m, averageTicket = 92.00m });

    [HttpGet("reports/summary")]
    public IActionResult ReportsSummary()
        => Ok(new { generatedToday = 7, exports = 3, scheduled = 5, lastReport = "DRE Gerencial Demo", status = "Atualizado" });

    [HttpGet("kiosk/status")]
    public IActionResult KioskStatus()
        => Ok(new { online = true, deviceCode = "KIOSK-DEMO-001", status = "Operando", currentStep = "Aguardando cliente", attendancesToday = 12, lastSync = DateTime.UtcNow });

    [HttpGet("kiosk/services")]
    public IActionResult KioskServices([FromQuery] string? deviceCode)
        => Ok(new { success = true, data = new[]
        {
            new { id = "demo-corte", name = "Corte Masculino", category = "Barbearia", description = "Corte moderno com acabamento profissional.", price = 45.00m, durationMinutes = 40, icon = "✂️", isAvailable = true },
            new { id = "demo-barba", name = "Barba Tradicional", category = "Barbearia", description = "Toalha quente, navalha e balm premium.", price = 35.00m, durationMinutes = 30, icon = "🪒", isAvailable = true },
            new { id = "demo-combo", name = "Corte + Barba", category = "Combo", description = "Experiência completa BarberSync.", price = 70.00m, durationMinutes = 60, icon = "💈", isAvailable = true },
            new { id = "demo-manicure", name = "Manicure", category = "Beleza", description = "Cuidado completo para unhas.", price = 40.00m, durationMinutes = 50, icon = "💅", isAvailable = true }
        }, deviceCode = string.IsNullOrWhiteSpace(deviceCode) ? "KIOSK-DEMO-001" : deviceCode });

    [HttpGet("kiosk/professionals")]
    public IActionResult KioskProfessionals([FromQuery] string? serviceId, [FromQuery] string? deviceCode)
        => Ok(new { success = true, data = new[]
        {
            new { id = "pro-rafael", name = "Rafael Barber", specialty = "Fade e barba", rating = 4.9m, estimatedWaitMinutes = 10 },
            new { id = "pro-lucas", name = "Lucas Navalha", specialty = "Corte clássico", rating = 4.8m, estimatedWaitMinutes = 15 },
            new { id = "pro-camila", name = "Camila Beauty", specialty = "Visagismo e estética", rating = 4.9m, estimatedWaitMinutes = 20 }
        }, serviceId, deviceCode = string.IsNullOrWhiteSpace(deviceCode) ? "KIOSK-DEMO-001" : deviceCode });

    [HttpPost("kiosk/client/find-by-phone")]
    public IActionResult KioskFindClient([FromBody] JsonElement payload)
        => Ok(new { success = true, message = "Cliente identificado.", data = new { id = "cli-demo", name = "Cliente Demo", phone = payload.TryGetProperty("phone", out var phone) ? phone.GetString() : "(11) 99999-9999" } });

    [HttpPost("kiosk/client/quick-register")]
    public IActionResult KioskQuickRegister([FromBody] JsonElement payload)
        => Ok(new { success = true, message = "Cadastro rápido realizado.", data = new { id = Guid.NewGuid(), protocol = $"KSK-{DateTime.UtcNow:HHmmss}", payload } });

    [HttpPost("kiosk/payment/mock")]
    public IActionResult KioskMockPayment([FromBody] JsonElement payload)
        => Ok(new { success = true, message = "Pagamento simulado aprovado.", data = new { status = "APPROVED", authorizationCode = $"PIX-{DateTime.UtcNow:HHmmss}", payload } });

    [HttpPost("kiosk/review")]
    public IActionResult KioskReview([FromBody] JsonElement payload)
        => Ok(new { success = true, message = "Avaliação recebida.", data = new { id = Guid.NewGuid(), payload } });

    [HttpPost("appointments/{id}/confirm")]
    public IActionResult ConfirmAppointment(string id) => AppointmentAction(id, "Confirmado");

    [HttpPost("appointments/{id}/check-in")]
    public IActionResult CheckInAppointment(string id) => AppointmentAction(id, "Check-in");

    [HttpPost("appointments/{id}/start")]
    public IActionResult StartAppointment(string id) => AppointmentAction(id, "Em atendimento");

    [HttpPost("appointments/{id}/finish")]
    public IActionResult FinishAppointment(string id) => AppointmentAction(id, "Atendimento finalizado");

    [HttpPost("appointments/{id}/cancel")]
    public IActionResult CancelAppointment(string id) => AppointmentAction(id, "Cancelado");

    [HttpPost("service-orders/{id}/pay")]
    public IActionResult PayServiceOrder(string id, [FromBody] JsonElement payload)
        => Ok(new { success = true, message = "Pagamento aprovado, estoque baixado e cashback gerado.", data = new { id, status = "Paid", receiptNumber = $"REC-{DateTime.UtcNow:yyyyMMddHHmmss}", payload } });

    [HttpPost("service-orders/{id}/close")]
    public IActionResult CloseServiceOrder(string id)
        => Ok(new { success = true, message = "Comanda fechada com recibo visual.", data = new { id, status = "Closed", closedAt = DateTime.UtcNow } });

    private IActionResult AppointmentAction(string id, string status)
        => Ok(new { success = true, message = $"Agendamento {status.ToLowerInvariant()}.", data = new { id, status, updatedAt = DateTime.UtcNow } });

}
