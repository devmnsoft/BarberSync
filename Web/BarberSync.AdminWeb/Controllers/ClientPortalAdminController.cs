using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;
namespace BarberSync.AdminWeb.Controllers;
[Authorize,Route("ClientPortalAdmin")]public sealed class ClientPortalAdminController:Controller{[HttpGet("")]public IActionResult Index()=>View();}
