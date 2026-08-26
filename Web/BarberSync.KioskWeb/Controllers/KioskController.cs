using Microsoft.AspNetCore.Mvc;

namespace BarberSync.KioskWeb.Controllers;

public class KioskController(IConfiguration configuration) : Controller
{
    public IActionResult Index(string? deviceCode)
    {
        var resolved = string.IsNullOrWhiteSpace(deviceCode)
            ? configuration["Kiosk:DeviceCode"] ?? string.Empty
            : deviceCode.Trim();
        ViewData["DeviceCode"] = resolved;
        ViewData["DeviceConfigured"] = !string.IsNullOrWhiteSpace(resolved);
        return View();
    }

    public IActionResult Services()
    {
        return View();
    }

    public IActionResult Client() => View();
    public IActionResult Professional() => View();
    public IActionResult Confirm() => View();
    public IActionResult Payment() => View();
    public IActionResult Success() => View();
    public IActionResult Summary() => View();
    public IActionResult Review() => View();
    public IActionResult Help() => View();
}
