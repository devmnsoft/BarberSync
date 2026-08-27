using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.PublicWeb.Controllers;

[Route("ClientPortal")]
public sealed class ClientPortalController(IHttpClientFactory clients):Controller
{
 private static readonly HashSet<string> Pages=["Home","Appointments","History","Consents","Budgets","TreatmentPlans","Payments","Benefits","Reviews","Preferences","Support"];
 [HttpGet("")]public IActionResult Index()=>RedirectToAction("Login");
 [HttpGet("Login")]public IActionResult Login()=>View();
 [HttpGet("{page}")]public IActionResult Page(string page){if(!Pages.Contains(page))return NotFound();if(!Request.Cookies.ContainsKey("BarberSync.ClientPortal"))return RedirectToAction("Login");ViewData["PortalPage"]=page;return View(page);}
 [HttpPost("proxy/{**path}")]public Task<IActionResult> ProxyPost(string path,CancellationToken ct)=>Proxy(path,HttpMethod.Post,ct);
 [HttpPut("proxy/{**path}")]public Task<IActionResult> ProxyPut(string path,CancellationToken ct)=>Proxy(path,HttpMethod.Put,ct);
 [HttpGet("proxy/{**path}")]public Task<IActionResult> ProxyGet(string path,CancellationToken ct)=>Proxy(path,HttpMethod.Get,ct);
 private async Task<IActionResult> Proxy(string path,HttpMethod method,CancellationToken ct)
 { if(path.Contains("..",StringComparison.Ordinal)||!path.StartsWith("api/client-portal/",StringComparison.Ordinal))return BadRequest();using var request=new HttpRequestMessage(method,path);if(method!=HttpMethod.Get)request.Content=new StreamContent(Request.Body){Headers={ContentType=new MediaTypeHeaderValue("application/json")}};var token=Request.Cookies["BarberSync.ClientPortal"];if(!string.IsNullOrWhiteSpace(token))request.Headers.Authorization=new("Bearer",token);using var response=await clients.CreateClient("BarberSyncApi").SendAsync(request,ct);var json=await response.Content.ReadAsStringAsync(ct);
   if(path.EndsWith("auth/verify-code",StringComparison.Ordinal)&&response.IsSuccessStatusCode){using var doc=JsonDocument.Parse(json);var access=doc.RootElement.GetProperty("data").GetProperty("accessToken").GetString();Response.Cookies.Append("BarberSync.ClientPortal",access!,new CookieOptions{HttpOnly=true,Secure=!HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment(),SameSite=SameSiteMode.Strict,MaxAge=TimeSpan.FromHours(8),IsEssential=true});}
   if(path.EndsWith("auth/logout",StringComparison.Ordinal))Response.Cookies.Delete("BarberSync.ClientPortal");return new ContentResult{StatusCode=(int)response.StatusCode,ContentType="application/json",Content=json}; }
}
