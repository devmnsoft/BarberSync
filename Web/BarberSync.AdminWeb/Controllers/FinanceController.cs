using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.AdminWeb.Controllers;
[Authorize, Route("Finance")]
public sealed class FinanceController : Controller
{
 [HttpGet("")] [HttpGet("Dashboard")] public IActionResult Index()=>View("Index");
 [HttpGet("AccountsPayable")] public IActionResult Payables()=>View();
 [HttpGet("AccountsReceivable")] public IActionResult Receivables()=>View();
 [HttpGet("Suppliers")] public IActionResult Suppliers()=>View();
 [HttpGet("Categories")] public IActionResult Categories()=>View();
 [HttpGet("Recurring")] public IActionResult Recurring()=>View();
 [HttpGet("Reconciliation")] public IActionResult Reconciliation()=>View();
 [HttpGet("CashFlow")] public IActionResult CashFlow()=>View("Reports");
 [HttpGet("Reports")] public IActionResult Reports()=>View();
}
