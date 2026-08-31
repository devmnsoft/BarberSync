using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.AdminWeb.Controllers;
[Authorize,Route("Team360")]
public sealed class Team360Controller:Controller
{
 [HttpGet("")] [HttpGet("Dashboard")] public IActionResult Index()=>View("Index");
 [HttpGet("{page:regex(Professionals|Schedules|Shifts|Availability|Absences|Vacations|Goals|Productivity|Commissions|Payroll|Permissions|Training|Performance|Audit|Reports|Settings)}")]
 public IActionResult Page(string page)=>View(page);
}
