using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.AdminWeb.Controllers;
[Authorize, Route("Analytics")]
public sealed class AnalyticsController : Controller
{
    [HttpGet("")] public IActionResult Index()=>RedirectToAction(nameof(Executive));
    [HttpGet("Executive")] public IActionResult Executive()=>Page("Executive","Visão executiva");
    [HttpGet("Operations")] public IActionResult Operations()=>Page("Operations","Operação");
    [HttpGet("Finance")] public IActionResult Finance()=>Page("Finance","Financeiro");
    [HttpGet("Team")] public IActionResult Team()=>Page("Team","Equipe");
    [HttpGet("Relationship")] public IActionResult Relationship()=>Page("Relationship","Relacionamento");
    [HttpGet("Inventory")] public IActionResult Inventory()=>Page("Inventory","Estoque");
    [HttpGet("Reports")] public IActionResult Reports()=>Page("Reports","Relatórios gerenciais");
    [HttpGet("Alerts")] public IActionResult Alerts()=>Page("Alerts","Alertas inteligentes");
    private IActionResult Page(string page,string title){ViewData["AnalyticsPage"]=page;ViewData["Title"]=title;return View(page);}
}
