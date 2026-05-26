using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login() => View();
}
