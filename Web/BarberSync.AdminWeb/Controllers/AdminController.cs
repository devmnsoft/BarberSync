using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

[Route("Admin")]
public class AdminController : Controller
{
    [HttpGet("")] public IActionResult Index() => Render("Dashboard");
    [HttpGet("Dashboard")] public IActionResult Dashboard() => Render("Dashboard");
    [HttpGet("Clients")] public IActionResult Clients() => Render("Clients");
    [HttpGet("Professionals")] public IActionResult Professionals() => Render("Professionals");
    [HttpGet("Services")] public IActionResult Services() => Render("Services");
    [HttpGet("Appointments")] public IActionResult Appointments() => Render("Appointments");
    [HttpGet("ServiceOrders")] public IActionResult ServiceOrders() => Render("ServiceOrders");
    [HttpGet("Cash")] public IActionResult Cash() => Render("Cash");
    [HttpGet("Payments")] public IActionResult Payments() => Render("Payments");
    [HttpGet("Products")] public IActionResult Products() => Render("Products");
    [HttpGet("Stock")] public IActionResult Stock() => Render("Stock");
    [HttpGet("Loyalty")] public IActionResult Loyalty() => Render("Loyalty");
    [HttpGet("Campaigns")] public IActionResult Campaigns() => Render("Campaigns");
    [HttpGet("Coupons")] public IActionResult Coupons() => Render("Coupons");
    [HttpGet("Reviews")] public IActionResult Reviews() => Render("Reviews");
    [HttpGet("Reports")] public IActionResult Reports() => Render("Reports");
    [HttpGet("Copilot")] public IActionResult Copilot() => Render("Copilot");
    [HttpGet("Kiosk")] public IActionResult Kiosk() => Render("Kiosk");
    [HttpGet("PublicSite")] public IActionResult PublicSite() => Render("PublicSite");
    [HttpGet("Settings")] public IActionResult Settings() => Render("Settings");
    [HttpGet("Help")] public IActionResult Help() => Render("Help");

    private IActionResult Render(string module) => View(module, BuildViewModel(module));

    private static AdminModuleViewModel BuildViewModel(string module)
        => new(module, $"BarberSync 2.0 • {module}", "Agenda, caixa, estoque, totem e inteligência em um só lugar.");

    public sealed record AdminModuleViewModel(string Module, string Title, string Subtitle);
}
