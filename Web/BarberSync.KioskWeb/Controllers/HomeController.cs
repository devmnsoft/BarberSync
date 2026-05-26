using Microsoft.AspNetCore.Mvc;
namespace BarberSync.KioskWeb.Controllers;
public class HomeController : Controller { public IActionResult Index() => RedirectToAction("Index", "Kiosk"); public IActionResult Error()=>View(); }
