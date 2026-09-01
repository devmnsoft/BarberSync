using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.AdminWeb.Controllers;
[Authorize,Route("Finance360")]
public sealed class Finance360Controller:Controller
{
 [HttpGet("")] [HttpGet("Dashboard")] public IActionResult Index()=>View("Index");
 [HttpGet("{page:regex(Receivables|Payables|Reconciliation|CashFlow|Dre|Commissions|Payroll|PartnerPayouts|Delinquency|Audit|Reports|Settings)}")]
 public IActionResult Page(string page)=>View(page);
}
