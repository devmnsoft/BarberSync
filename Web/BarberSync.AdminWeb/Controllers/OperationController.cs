using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

/// <summary>Authenticated entry points for the integrated daily operation workspace.</summary>
[Authorize]
[Route("Operation")]
public sealed class OperationController : Controller
{
    [HttpGet("")]
    [HttpGet("Today")]
    public IActionResult Index() => View();

    [HttpGet("ServiceOrders")]
    public IActionResult ServiceOrders() => Redirect("/Admin/ServiceOrders");

    [HttpGet("ServiceOrders/{id:guid}")]
    public IActionResult ServiceOrder(Guid id) => Redirect($"/Admin/ServiceOrders?orderId={id}");

    [HttpGet("Cash")]
    public IActionResult Cash() => Redirect("/Admin/Cash");
}
