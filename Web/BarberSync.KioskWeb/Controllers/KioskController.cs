using Microsoft.AspNetCore.Mvc;

namespace BarberSync.KioskWeb.Controllers;

public class KioskController(IConfiguration configuration) : Controller
{
    private const string DefaultDeviceCode = "KIOSK-DEMO-001";

    public IActionResult Index(string? deviceCode)
    {
        ViewData["DeviceCode"] = string.IsNullOrWhiteSpace(deviceCode) ? DefaultDeviceCode : deviceCode.Trim();
        ViewBag.ApiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:8080";
        return View();
    }

    public IActionResult Services()
    {
        ViewBag.ApiBaseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:8080";
        return View();
    }

    public IActionResult Client() => View();
    public IActionResult Professional() => View();
    public IActionResult Confirm() => View();
    public IActionResult Payment() => View();
    public IActionResult Success() => View();
    public IActionResult Review() => View();
}
