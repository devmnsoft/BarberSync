using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

public class LegacyRedirectController : Controller
{
    [HttpGet("Dashboard")] public IActionResult Dashboard() => RedirectToActionPermanent("Dashboard", "Admin");
    [HttpGet("Clients")] public IActionResult Clients() => RedirectToActionPermanent("Clients", "Admin");
    [HttpGet("Professionals")] public IActionResult Professionals() => RedirectToActionPermanent("Professionals", "Admin");
    [HttpGet("Services")] public IActionResult Services() => RedirectToActionPermanent("Services", "Admin");
    [HttpGet("Appointments")] public IActionResult Appointments() => RedirectToActionPermanent("Appointments", "Admin");
    [HttpGet("ServiceOrders")] public IActionResult ServiceOrders() => RedirectToActionPermanent("ServiceOrders", "Admin");
    [HttpGet("Stock")] public IActionResult Stock() => RedirectToActionPermanent("Stock", "Admin");
    [HttpGet("Products")] public IActionResult Products() => RedirectToActionPermanent("Products", "Admin");
    [HttpGet("Loyalty")] public IActionResult Loyalty() => RedirectToActionPermanent("Loyalty", "Admin");
    [HttpGet("Campaigns")] public IActionResult Campaigns() => RedirectToActionPermanent("Campaigns", "Admin");
    [HttpGet("Coupons")] public IActionResult Coupons() => RedirectToActionPermanent("Coupons", "Admin");
    [HttpGet("Reviews")] public IActionResult Reviews() => RedirectToActionPermanent("Reviews", "Admin");
    [HttpGet("Copilot")] public IActionResult Copilot() => RedirectToActionPermanent("Copilot", "Admin");
    [HttpGet("Reports")] public IActionResult Reports() => RedirectToActionPermanent("Reports", "Admin");
    [HttpGet("Settings")] public IActionResult Settings() => RedirectToActionPermanent("Settings", "Admin");
}
