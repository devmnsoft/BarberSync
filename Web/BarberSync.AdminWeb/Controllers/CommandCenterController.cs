using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.AdminWeb.Controllers;
[Authorize,Route("CommandCenter")]
public sealed class CommandCenterController:Controller
{
 [HttpGet("")][HttpGet("Dashboard")]public IActionResult Index()=>Page("Index","Visão geral");
 [HttpGet("{section:regex(Executive|Operations|Health|Alerts|Incidents|Tasks|Integrations|Readiness|Audit|Reports)}")]public IActionResult Section(string section)=>Page(section is "Integrations" or "Readiness" or "Audit"?"Health":section,section);
 private IActionResult Page(string view,string title){ViewData["Title"]=$"{title} · Central de Controle";ViewData["CommandCenterPage"]=title;return View(view);}
}
