using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

public class HelpController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
