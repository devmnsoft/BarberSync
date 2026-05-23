using BarberSync.Api.Finance.Models;
using BarberSync.Api.Finance.Services;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Finance.Controllers;

[ApiController]
[Route("api/fiscal")]
public class FiscalController(FinanceDataStore db) : ControllerBase
{
    [HttpGet("settings/{tenantId:guid}")]
    public ActionResult<FiscalSettingsDto> GetSettings(Guid tenantId)
        => db.FiscalSettings.TryGetValue(tenantId, out var settings) ? Ok(settings) : NotFound();

    [HttpPost("settings")]
    public ActionResult<FiscalSettingsDto> UpsertSettings([FromBody] FiscalSettingsDto dto)
    {
        db.FiscalSettings[dto.TenantId] = dto;
        return Ok(dto);
    }

    [HttpGet("export/{tenantId:guid}")]
    public IActionResult Export(Guid tenantId)
    {
        var lines = db.Receipts.Where(r => r.TenantId == tenantId).Select(r => $"{r.Number};{r.IssuedAtUtc:O};{r.CustomerName};{r.Total:F2};{r.IsCanceled}");
        return File(System.Text.Encoding.UTF8.GetBytes(string.Join("\n", lines)), "text/csv", "fiscal-export.csv");
    }

    [HttpPost("receipts/{id:guid}/cancel")]
    public IActionResult Cancel(Guid id, [FromQuery] string reason = "Cancelado")
    {
        var idx = db.Receipts.FindIndex(r => r.Id == id);
        if (idx < 0) return NotFound();
        db.Receipts[idx] = db.Receipts[idx] with { IsCanceled = true, CancellationReason = reason };
        return Ok(db.Receipts[idx]);
    }

    [HttpPost("receipts/{id:guid}/reissue")]
    public IActionResult Reissue(Guid id)
    {
        var old = db.Receipts.FirstOrDefault(r => r.Id == id);
        if (old is null) return NotFound();
        var req = new GenerateReceiptRequest(old.TenantId, old.BranchId, old.CustomerName, "", null, "REISSUE", old.PaymentMethod, old.CashbackUsed, old.Items);
        return Ok(db.GenerateReceipt(req));
    }
}

[ApiController]
[Route("api/receipts")]
public class ReceiptsController(FinanceDataStore db) : ControllerBase
{
    [HttpPost("generate")]
    public ActionResult<ReceiptDto> Generate([FromBody] GenerateReceiptRequest request) => Ok(db.GenerateReceipt(request));

    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id) => db.Receipts.FirstOrDefault(r => r.Id == id) is { } r ? Ok(r) : NotFound();

    [HttpGet("{id:guid}/pdf")]
    public IActionResult Pdf(Guid id)
    {
        var r = db.Receipts.FirstOrDefault(x => x.Id == id);
        if (r is null) return NotFound();
        var content = $"RECIBO {r.Series}-{r.Number}\nCliente: {r.CustomerName}\nTotal: {r.Total:F2}\nHash: {r.Hash}";
        return File(System.Text.Encoding.UTF8.GetBytes(content), "application/pdf", $"recibo-{r.Number}.pdf");
    }

    [HttpGet("{id:guid}/validate")]
    public IActionResult Validate(Guid id) => db.Receipts.Any(r => r.Id == id) ? Ok(new { valid = true }) : NotFound(new { valid = false });
}

[ApiController]
[Route("api/accounts-payable")]
public class AccountsPayableController(FinanceDataStore db) : ControllerBase
{
    [HttpGet] public IActionResult List() => Ok(db.Payables);
    [HttpPost] public IActionResult Create([FromBody] AccountPayableDto dto) { db.Payables.Add(dto); return Ok(dto); }
    [HttpPost("{id:guid}/pay")]
    public IActionResult Pay(Guid id, [FromBody] ReceivePayableRequest request)
    {
        var i = db.Payables.FindIndex(x => x.Id == id); if (i < 0) return NotFound();
        db.Payables[i] = db.Payables[i] with { Status = "PAID" };
        db.CashflowEntries.Add(new CashflowEntryDto(Guid.NewGuid(), db.Payables[i].TenantId, db.Payables[i].BranchId, request.PaidAtUtc, "OUT", request.Amount, "PAYABLE"));
        return Ok(db.Payables[i]);
    }
}

[ApiController]
[Route("api/financial-dashboard")]
public class FinancialDashboardController(FinanceDataStore db) : ControllerBase
{
    [HttpGet("summary/{tenantId:guid}")]
    public IActionResult Summary(Guid tenantId)
    {
        var now = DateTime.UtcNow;
        var revenue = db.CashflowEntries.Where(c => c.TenantId == tenantId && c.Type == "IN" && c.DateUtc.Month == now.Month && c.DateUtc.Year == now.Year).Sum(c => c.Amount);
        var expenses = db.CashflowEntries.Where(c => c.TenantId == tenantId && c.Type == "OUT" && c.DateUtc.Month == now.Month && c.DateUtc.Year == now.Year).Sum(c => c.Amount);
        var margin = revenue == 0 ? 0 : ((revenue - expenses) / revenue) * 100;
        var projected = db.CashflowEntries.Where(c => c.TenantId == tenantId).Sum(c => c.Type == "IN" ? c.Amount : -c.Amount);
        return Ok(new FinancialDashboardSummaryDto(revenue, expenses, revenue - expenses, margin, db.Payables.Count(p => p.TenantId == tenantId && p.Status != "PAID" && p.DueDateUtc < now), db.Receivables.Count(r => r.TenantId == tenantId && r.Status != "RECEIVED" && r.DueDateUtc < now), db.TaxEstimates.Where(t => t.TenantId == tenantId && t.PeriodStartUtc.Month == now.Month).Sum(t => t.EstimatedAmount), db.Receipts.Count(r => r.TenantId == tenantId), projected));
    }
}
