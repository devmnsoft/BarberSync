using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.AdminWeb.Controllers;
[Authorize, Route("Inventory")]
public sealed class InventoryController : Controller
{
    [HttpGet("")][HttpGet("Dashboard")] public IActionResult Index()=>View("Index");
    [HttpGet("Products")][HttpGet("Categories")] public IActionResult Products()=>View();
    [HttpGet("Purchases")] public IActionResult Purchases()=>View();
    [HttpGet("Receiving")] public IActionResult Receiving()=>View();
    [HttpGet("Counts")][HttpGet("Movements")] public IActionResult Counts()=>View();
    [HttpGet("Transfers")] public IActionResult Transfers()=>View();
    [HttpGet("Replenishment")] public IActionResult Replenishment()=>View();
    [HttpGet("ServiceInputs")] public IActionResult ServiceInputs()=>View();
    [HttpGet("Reports")] public IActionResult Reports()=>View();
}
