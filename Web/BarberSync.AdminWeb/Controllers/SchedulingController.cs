using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

[Authorize]
[Route("Scheduling")]
public sealed class SchedulingController : Controller
{
    [HttpGet("")] public IActionResult Index() => View();
    [HttpGet("Calendar")] public IActionResult Calendar() => View();
    [HttpGet("Waitlist")] public IActionResult Waitlist() => View();
    [HttpGet("Policies"), Authorize(Roles = "Owner,SuperAdmin,Admin")] public IActionResult Policies() => View();
    [HttpGet("Resources"), Authorize(Roles = "Owner,SuperAdmin,Admin")] public IActionResult Resources() => View();
}
