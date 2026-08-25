using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

[Authorize, Route("Team")]
public sealed class TeamController : Controller
{
    [HttpGet("")] public IActionResult Index()=>View();
    [HttpGet("Professionals")] public IActionResult Professionals()=>View("Index");
    [HttpGet("Professionals/{id:guid}")] public IActionResult Professional(Guid id)=>View(id);
    [HttpGet("Schedules")] public IActionResult Schedules()=>View();
    [HttpGet("Commissions")] public IActionResult Commissions()=>View();
    [HttpGet("Goals")] public IActionResult Goals()=>View();
    [HttpGet("Payouts")] public IActionResult Payouts()=>View();
    [HttpGet("TimeOff")] public IActionResult TimeOff()=>View("Schedules");
}
