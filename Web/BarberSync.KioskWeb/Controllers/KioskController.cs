using Microsoft.AspNetCore.Mvc;
namespace BarberSync.KioskWeb.Controllers;
public class KioskController : Controller { public IActionResult Index(string? deviceCode){ ViewData["DeviceCode"] = string.IsNullOrWhiteSpace(deviceCode) ? "KIOSK-DEMO-001" : deviceCode; return View(); } public IActionResult Services()=>View(); public IActionResult Client()=>View(); public IActionResult Confirm()=>View(); public IActionResult Success()=>View(); }
