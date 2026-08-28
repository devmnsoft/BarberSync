using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.AdminWeb.Controllers;
[Authorize,Route("Quality")]
public sealed class QualityController:Controller
{
 [HttpGet("")] [HttpGet("Dashboard")] public IActionResult Index()=>Page("Index","Visão geral");
 [HttpGet("Reviews")] public IActionResult Reviews()=>Page("Reviews","Avaliações");
 [HttpGet("Nps")] public IActionResult Nps()=>Page("Nps","NPS");
 [HttpGet("Recovery")] public IActionResult Recovery()=>Page("Recovery","Recuperação");
 [HttpGet("FollowUps")] public IActionResult FollowUps()=>Page("FollowUps","Follow-ups");
 [HttpGet("Retention")] public IActionResult Retention()=>Page("Retention","Retenção");
 [HttpGet("Reputation")] public IActionResult Reputation()=>Page("Reputation","Reputação");
 [HttpGet("Campaigns")] public IActionResult Campaigns()=>Redirect("/Communication/Campaigns?origin=quality");
 [HttpGet("Reports")] public IActionResult Reports()=>Page("Reports","Relatórios");
 [HttpGet("Settings")] public IActionResult Settings()=>Page("Settings","Configurações");
 private IActionResult Page(string view,string title){ViewData["Title"]=$"{title} · Qualidade & Retenção";ViewData["QualityPage"]=view;return View(view);}
}
