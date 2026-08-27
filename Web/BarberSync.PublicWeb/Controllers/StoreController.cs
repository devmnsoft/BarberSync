using Microsoft.AspNetCore.Mvc;
namespace BarberSync.PublicWeb.Controllers;
[Route("Store")]
public sealed class StoreController:Controller
{
 [HttpGet("")] public IActionResult Index()=>View();
 [HttpGet("Plans")] public IActionResult Plans()=>View();
 [HttpGet("GiftCards")] public IActionResult GiftCards()=>View();
 [HttpGet("Checkout")] public IActionResult Checkout()=>View();
}
