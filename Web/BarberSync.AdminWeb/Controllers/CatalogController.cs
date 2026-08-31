using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.AdminWeb.Controllers;
[Authorize,Route("Catalog")]
public sealed class CatalogController:Controller
{
 [HttpGet("")][HttpGet("Dashboard")]public IActionResult Index()=>Open("Index","Dashboard");
 [HttpGet("{section:regex(Services|Products|Combos|Packages|PricingRules|Margins|Commissions|Promotions|PriceSimulator|Simulator|Audit|Reports|Settings)}")]
 public IActionResult Section(string section)=>Open(section is "PriceSimulator" or "Promotions" or "Settings"?"Simulator":section,section);
 private IActionResult Open(string view,string section){ViewData["Title"]=$"{section} · Catálogo & Precificação";ViewData["CatalogPage"]=section;return View(view);}
}
