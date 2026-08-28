using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.AdminWeb.Controllers;
[Authorize,Route("Marketing")]
public sealed class MarketingController:Controller
{
 [HttpGet("")][HttpGet("Dashboard")]public IActionResult Index()=>Page("Index","Dashboard");
 [HttpGet("Segments")]public IActionResult Segments()=>Page("Segments","Segmentos"); [HttpGet("Campaigns")]public IActionResult Campaigns()=>Page("Campaigns","Campanhas");
 [HttpGet("Journeys")]public IActionResult Journeys()=>Page("Journeys","Jornadas"); [HttpGet("LandingPages")]public IActionResult LandingPages()=>Page("LandingPages","Landing pages");
 [HttpGet("PromoLinks")]public IActionResult PromoLinks()=>Page("PromoLinks","Links promocionais"); [HttpGet("QrCodes")]public IActionResult QrCodes()=>Page("QrCodes","QR Codes");
 [HttpGet("Calendar")]public IActionResult Calendar()=>Page("Calendar","Calendário"); [HttpGet("Experiments")]public IActionResult Experiments()=>Page("Experiments","Experimentos");
 [HttpGet("Reports")]public IActionResult Reports()=>Page("Reports","Relatórios"); [HttpGet("Settings")]public IActionResult Settings()=>Page("Settings","Configurações");
 IActionResult Page(string view,string title){ViewData["Title"]=$"{title} · Marketing Studio";ViewData["MarketingPage"]=view;return View(view);}
}
