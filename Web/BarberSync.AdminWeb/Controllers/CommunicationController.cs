using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.AdminWeb.Controllers;
[Authorize,Route("Communication")]
public sealed class CommunicationController:Controller
{
 [HttpGet("")] [HttpGet("Dashboard")] public IActionResult Index()=>Page("Index","Dashboard");
 [HttpGet("Templates")] public IActionResult Templates()=>Page("Templates","Templates");
 [HttpGet("Campaigns")] public IActionResult Campaigns()=>Page("Campaigns","Campanhas");
 [HttpGet("Automations")] public IActionResult Automations()=>Page("Automations","Automações");
 [HttpGet("Outbox")] public IActionResult Outbox()=>Page("Outbox","Outbox");
 [HttpGet("Inbox")] public IActionResult Inbox()=>Page("Inbox","Inbox");
 [HttpGet("Preferences")] public IActionResult Preferences()=>Page("Preferences","Preferências");
 [HttpGet("Reports")] public IActionResult Reports()=>Page("Reports","Relatórios");
 private IActionResult Page(string view,string page){ViewData["Title"]=$"Comunicação — {page}";ViewData["CommunicationPage"]=page;return View(view);}
}
