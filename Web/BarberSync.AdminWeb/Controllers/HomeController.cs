using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Admin");
    public IActionResult Error() => View();
}
