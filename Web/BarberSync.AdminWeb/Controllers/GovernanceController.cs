using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.AdminWeb.Controllers;
[Authorize,Route("Governance")]
public sealed class GovernanceController:Controller
{
 [HttpGet("")] [HttpGet("Dashboard")] public IActionResult Index()=>Page("Index","Dashboard");
 [HttpGet("Tenants")] public IActionResult Tenants()=>Page("Tenants","Empresas");
 [HttpGet("Branches")] public IActionResult Branches()=>Page("Branches","Filiais");
 [HttpGet("Users")] public IActionResult Users()=>Page("Users","Usuários");
 [HttpGet("Roles")] public IActionResult Roles()=>Page("Roles","Perfis");
 [HttpGet("Permissions")] public IActionResult Permissions()=>Page("Permissions","Permissões");
 [HttpGet("Plans")] public IActionResult Plans()=>Page("Plans","Planos");
 [HttpGet("Subscription")] [HttpGet("Subscriptions")] public IActionResult Subscription()=>Page("Subscription","Assinatura");
 [HttpGet("Modules")] public IActionResult Modules()=>Page("Modules","Módulos");
 [HttpGet("Audit")] public IActionResult Audit()=>Page("Audit","Auditoria");
 [HttpGet("Security")] public IActionResult Security()=>Page("Security","Segurança");
 [HttpGet("Privacy")] public IActionResult Privacy()=>Page("Privacy","LGPD e privacidade");
 [HttpGet("Onboarding")] public IActionResult Onboarding()=>Page("Onboarding","Onboarding");
 [HttpGet("Settings")] public IActionResult Settings()=>Page("Settings","Configurações");
 private IActionResult Page(string view,string page){ViewData["Title"]=$"Governança — {page}";ViewData["GovernancePage"]=page;return View(view);}
}
