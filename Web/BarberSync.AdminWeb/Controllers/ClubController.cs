using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.AdminWeb.Controllers;
[Authorize,Route("Club")]
public sealed class ClubController:Controller
{
 [HttpGet("")] [HttpGet("Dashboard")] public IActionResult Index()=>View("Index");
 [HttpGet("{section:regex(Plans|Memberships|Wallets|GiftCards|Vouchers|Combos|OnlineSales|Reports|Benefits|Settings)}")] public IActionResult Section(string section)=>View(section is "Benefits" or "Settings"?"Index":section);
}
