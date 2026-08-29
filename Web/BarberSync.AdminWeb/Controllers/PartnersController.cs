using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.AdminWeb.Controllers;
[Authorize,Route("Partners")]
public sealed class PartnersController:Controller
{
 [HttpGet("")][HttpGet("Dashboard")] public IActionResult Index()=>Page("Index","Dashboard");
 [HttpGet("{section:regex(Directory|Affiliates|Referrals|Commissions|Payouts|Contracts|Coupons|Links|Marketplace|Suppliers|Reports|Settings)}")] public IActionResult Section(string section)=>Page(section,section);
 IActionResult Page(string view,string title){ViewData["Title"]=$"{title} · Marketplace & Parceiros";ViewData["PartnersPage"]=view;return View(view);}
}
