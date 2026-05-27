using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

public class LegacyRedirectController : Controller
{
    [HttpGet("/")]
    public IActionResult Root() => RedirectToAction("Index", "Admin");

    [HttpGet("/Dashboard")] public IActionResult Dashboard() => Redirect("/Admin/Dashboard");
    [HttpGet("/Clients")] public IActionResult Clients() => Redirect("/Admin/Clients");
    [HttpGet("/Professionals")] public IActionResult Professionals() => Redirect("/Admin/Professionals");
    [HttpGet("/Services")] public IActionResult Services() => Redirect("/Admin/Services");
    [HttpGet("/Appointments")] public IActionResult Appointments() => Redirect("/Admin/Appointments");
    [HttpGet("/ServiceOrders")] public IActionResult ServiceOrders() => Redirect("/Admin/ServiceOrders");
    [HttpGet("/Stock")] public IActionResult Stock() => Redirect("/Admin/Stock");
    [HttpGet("/Loyalty")] public IActionResult Loyalty() => Redirect("/Admin/Loyalty");
    [HttpGet("/Campaigns")] public IActionResult Campaigns() => Redirect("/Admin/Campaigns");
    [HttpGet("/Coupons")] public IActionResult Coupons() => Redirect("/Admin/Coupons");
    [HttpGet("/Reviews")] public IActionResult Reviews() => Redirect("/Admin/Reviews");
    [HttpGet("/Copilot")] public IActionResult Copilot() => Redirect("/Admin/Copilot");
}
