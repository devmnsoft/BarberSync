using Microsoft.AspNetCore.Mvc;
namespace BarberSync.PublicWeb.Controllers;
public sealed class BookingController : Controller
{
    [HttpGet("agendar")] public IActionResult Index() => View();
}
