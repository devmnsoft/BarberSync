using Microsoft.AspNetCore.Mvc;
namespace BarberSync.PublicWeb.Controllers;
[Route("marketplace")]
public sealed class MarketplaceController:Controller
{
 [HttpGet("")] public IActionResult Index()=>Page("Index","Marketplace");
 [HttpGet("services")] public IActionResult Services()=>Page("Services","Serviços");
 [HttpGet("products")] public IActionResult Products()=>Page("Products","Produtos");
 [HttpGet("packages")] public IActionResult Packages()=>Page("Packages","Pacotes e combos");
 [HttpGet("partners/{slug}")] public IActionResult Partner(string slug){ViewData["PartnerSlug"]=slug;return Page("Partner","Parceiro");}
 IActionResult Page(string view,string title){ViewData["Title"]=title;ViewData["MarketplaceType"]=view;return View(view);}
}
