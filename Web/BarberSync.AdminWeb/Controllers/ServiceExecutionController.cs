using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.AdminWeb.Controllers;
[Authorize,Route("ServiceExecution")]
public sealed class ServiceExecutionController:Controller{
 [HttpGet("")][HttpGet("Dashboard")]public IActionResult Index()=>Open("Index","Dashboard");
 [HttpGet("{section:regex(Today|CheckIn|Orders|Chair|Checkout|Cashier|Commissions|InventoryConsumption|Audit|Reports)}")]public IActionResult Section(string section)=>Open(section,section);
 private IActionResult Open(string view,string section){ViewData["Title"]=$"{section} · Atendimento 360";ViewData["ServiceExecutionPage"]=section;return View(view);}}
