using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

[Route("Admin")]
public class AdminController : Controller
{
    [HttpGet("")]
    public IActionResult Index() => View("Index", BuildViewModel("Dashboard"));

    [HttpGet("{module}")]
    public IActionResult Module([FromRoute] string module) => View("Index", BuildViewModel(module));

    private static AdminModuleViewModel BuildViewModel(string module)
    {
        var normalized = string.IsNullOrWhiteSpace(module) ? "Dashboard" : module.Trim();
        return new AdminModuleViewModel(normalized, $"BarberSync 2.0 • {normalized}",
            "Agenda, caixa, estoque, totem e inteligência em um só lugar.");
    }

    public sealed record AdminModuleViewModel(string Module, string Title, string Subtitle);
}
