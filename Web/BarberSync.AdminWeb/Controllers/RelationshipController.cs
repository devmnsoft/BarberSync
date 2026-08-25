using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

[Authorize, Route("Relationship")]
public sealed class RelationshipController : Controller
{
    [HttpGet("")] public IActionResult Index() => View();
    [HttpGet("Clients")] public IActionResult Clients() => Redirect("/Admin/Clients");
    [HttpGet("Clients/{id:guid}")] public IActionResult Client(Guid id) => Redirect($"/Admin/Clients/{id}");
    [HttpGet("Packages")] public IActionResult Packages() => Redirect("/Admin/Packages");
    [HttpGet("Coupons")] public IActionResult Coupons() => Redirect("/Admin/Coupons");
    [HttpGet("Loyalty")] public IActionResult Loyalty() => Redirect("/Admin/Loyalty");
    [HttpGet("Campaigns")] public IActionResult Campaigns() => Redirect("/Admin/Campaigns");
}
