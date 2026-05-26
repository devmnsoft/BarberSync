using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

public class HelpController : Controller
{
    public IActionResult Index() => View();
}
