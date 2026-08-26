using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.AdminWeb.Controllers;
[Authorize, Route("AiOperations")]
public sealed class AiOperationsController : Controller
{
 [HttpGet("")] [HttpGet("Dashboard")] public IActionResult Index()=>Page("Index","Dashboard");
 [HttpGet("Cameras")] public IActionResult Cameras()=>Page("Cameras","Cameras");
 [HttpGet("Zones")] public IActionResult Zones()=>Page("Zones","Zones");
 [HttpGet("Rules")] public IActionResult Rules()=>Page("Rules","Rules");
 [HttpGet("ReviewQueue")] public IActionResult ReviewQueue()=>Page("ReviewQueue","ReviewQueue");
 [HttpGet("Suggestions")] public IActionResult Suggestions()=>Page("ReviewQueue","ReviewQueue");
 [HttpGet("Evidence")] public IActionResult Evidence()=>Page("Evidence","Evidence");
 [HttpGet("Reports")] public IActionResult Reports()=>Page("Reports","Reports");
 [HttpGet("Settings")] [Authorize(Roles="Owner,SuperAdmin,Admin")] public IActionResult Settings()=>Page("Settings","Settings");
 private IActionResult Page(string view,string page){ViewData["Title"]="IA Operacional";ViewData["AiPage"]=page;return View(view);}
}
