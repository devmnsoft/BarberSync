using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.AdminWeb.Controllers;
[Authorize,Route("Inventory360")]
public sealed class Inventory360Controller:Controller
{
 [HttpGet("")][HttpGet("Dashboard")]public IActionResult Index()=>Page("Index","dashboard","Estoque & Compras 360");
 [HttpGet("Products")]public IActionResult Products()=>Page("Products","products","Produtos"); [HttpGet("Supplies")]public IActionResult Supplies()=>Page("Supplies","supplies","Insumos"); [HttpGet("ServiceInputs")]public IActionResult ServiceInputs()=>Page("ServiceInputs","service-inputs","Insumos por serviço"); [HttpGet("Stock")]public IActionResult Stock()=>Page("Stock","stock","Estoque atual"); [HttpGet("Batches")]public IActionResult Batches()=>Page("Batches","batches","Lotes e validade"); [HttpGet("Purchases")]public IActionResult Purchases()=>Page("Purchases","purchases","Compras e recebimento"); [HttpGet("Suppliers")]public IActionResult Suppliers()=>Page("Suppliers","suppliers","Fornecedores"); [HttpGet("Transfers")]public IActionResult Transfers()=>Page("Transfers","transfers","Transferências"); [HttpGet("InventoryCounts")][HttpGet("Losses")]public IActionResult InventoryCounts()=>Page("InventoryCounts","counts","Inventário e perdas"); [HttpGet("Replenishment")]public IActionResult Replenishment()=>Page("Replenishment","replenishment","Reposição inteligente"); [HttpGet("Costing")]public IActionResult Costing()=>Page("Costing","costing","CMV e custos"); [HttpGet("Audit")]public IActionResult Audit()=>Page("Audit","audit","Auditoria"); [HttpGet("Reports")][HttpGet("Settings")]public IActionResult Reports()=>Page("Reports","reports","Relatórios");
 private IActionResult Page(string view,string page,string title){ViewData["Inventory360Page"]=page;ViewData["Title"]=title;return View(view);}
}
