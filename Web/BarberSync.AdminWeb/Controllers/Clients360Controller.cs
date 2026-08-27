using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

[Authorize, Route("Clients360")]
public sealed class Clients360Controller : Controller
{
    [HttpGet("")] public IActionResult Index() => View();
    [HttpGet("{actionName:regex(Profile|TechnicalSheet|Anamnesis|VisualHistory|Consents|Budgets|TreatmentPlans|FollowUps)}/{clientId:guid}")]
    public IActionResult Workspace(string actionName, Guid clientId)
    {
        ViewData["ClientId"] = clientId;
        ViewData["Section"] = actionName;
        return View(actionName);
    }
}
