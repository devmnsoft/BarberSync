using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

[Authorize(Policy="PlatformAccess"),Route("Platform")]
public sealed class PlatformController(IHttpClientFactory clients):Controller
{
    [HttpGet("")]public IActionResult Index()=>View();
    [HttpGet("Tenants")]public IActionResult Tenants()=>View();
    [HttpGet("Tenants/{id:guid}")]public IActionResult Tenant(Guid id){ViewData["TenantId"]=id;return View();}
    [HttpGet("Modules")]public IActionResult Modules()=>View();
    [HttpGet("Prices")]public IActionResult Prices()=>View();
    [HttpGet("Contracts")]public IActionResult Contracts()=>View();
    [HttpGet("Audit")]public IActionResult Audit()=>View();
    [HttpGet("Users")]public IActionResult Users()=>Section("Usuários da Plataforma","Operadores BarberSync são separados dos usuários dos clientes.");
    [HttpGet("Plans")]public IActionResult Plans()=>Section("Planos","Planos base e módulos incluídos.");
    [HttpGet("Billing")]public IActionResult Billing()=>Section("Cobranças","Nenhum pagamento é aprovado sem um provider real configurado.");
    [HttpGet("Usage")]public IActionResult Usage()=>Section("Uso da Plataforma","Agregados diários por módulo, cliente e período.");
    [HttpGet("Health")]public IActionResult Health()=>Section("Saúde do Sistema","Disponibilidade e readiness sem dados fabricados.");
    [HttpGet("Settings")]public IActionResult Settings()=>Section("Configurações da Plataforma","Governança comercial e operacional do Control Plane.");

    [HttpGet("Data/Dashboard")]public Task<IActionResult> DashboardData(CancellationToken ct)=>ForwardGet("/api/platform/dashboard"+Request.QueryString,ct);
    [HttpGet("Data/Tenants")]public Task<IActionResult> TenantsData(CancellationToken ct)=>ForwardGet("/api/platform/tenants"+Request.QueryString,ct);
    [HttpGet("Data/Tenants/{id:guid}")]public Task<IActionResult> TenantData(Guid id,CancellationToken ct)=>ForwardGet($"/api/platform/tenants/{id}",ct);
    [HttpGet("Data/Tenants/{id:guid}/Users")]public Task<IActionResult> TenantUsersData(Guid id,CancellationToken ct)=>ForwardGet($"/api/platform/tenants/{id}/users"+Request.QueryString,ct);
    [HttpGet("Data/Tenants/{id:guid}/Roles")]public Task<IActionResult> TenantRolesData(Guid id,CancellationToken ct)=>ForwardGet($"/api/platform/tenants/{id}/roles",ct);
    [HttpGet("Data/Tenants/{id:guid}/Branches")]public Task<IActionResult> TenantBranchesData(Guid id,CancellationToken ct)=>ForwardGet($"/api/platform/tenants/{id}/branches",ct);
    [HttpGet("Data/Tenants/{id:guid}/Usage")]public Task<IActionResult> TenantUsageData(Guid id,CancellationToken ct)=>ForwardGet($"/api/platform/tenants/{id}/usage"+Request.QueryString,ct);
    [HttpGet("Data/Tenants/{id:guid}/Modules")]public Task<IActionResult> TenantModulesData(Guid id,CancellationToken ct)=>ForwardGet($"/api/platform/tenants/{id}/modules",ct);
    [HttpGet("Data/Modules")]public Task<IActionResult> ModulesData(CancellationToken ct)=>ForwardGet("/api/platform/modules",ct);
    [HttpGet("Data/Prices")]public Task<IActionResult> PricesData(CancellationToken ct)=>ForwardGet("/api/platform/module-prices"+Request.QueryString,ct);
    [HttpGet("Data/Contracts")]public Task<IActionResult> ContractsData(CancellationToken ct)=>ForwardGet("/api/platform/contracts"+Request.QueryString,ct);
    [HttpGet("Data/Audit")]public Task<IActionResult> AuditData(CancellationToken ct)=>ForwardGet("/api/platform/audit"+Request.QueryString,ct);
    [HttpPost("Data/Modules"),ValidateAntiForgeryToken]public Task<IActionResult> CreateModule([FromBody]JsonElement body,CancellationToken ct)=>ForwardJson(HttpMethod.Post,"/api/platform/modules",body,ct);
    [HttpPut("Data/Modules/{id:guid}"),ValidateAntiForgeryToken]public Task<IActionResult> UpdateModule(Guid id,[FromBody]JsonElement body,CancellationToken ct)=>ForwardJson(HttpMethod.Put,$"/api/platform/modules/{id}",body,ct);
    [HttpPost("Data/Prices"),ValidateAntiForgeryToken]public Task<IActionResult> CreatePrice([FromBody]JsonElement body,CancellationToken ct)=>ForwardJson(HttpMethod.Post,"/api/platform/module-prices",body,ct);
    [HttpPut("Data/Prices/{id:guid}"),ValidateAntiForgeryToken]public Task<IActionResult> UpdatePrice(Guid id,[FromBody]JsonElement body,CancellationToken ct)=>ForwardJson(HttpMethod.Put,$"/api/platform/module-prices/{id}",body,ct);

    [HttpPost("Scope"),ValidateAntiForgeryToken]
    public async Task<IActionResult> EnterScope(Guid tenantId,string reason,CancellationToken ct)
    {
        if(tenantId==Guid.Empty||string.IsNullOrWhiteSpace(reason))return Redirect($"/Platform/Tenants/{tenantId}?scopeError=reason");
        var client=AuthenticatedClient();using var content=new StringContent(JsonSerializer.Serialize(new{tenantId,branchId=(Guid?)null,reason}),Encoding.UTF8,"application/json");var response=await client.PostAsync("/api/platform/scope",content,ct);if(!response.IsSuccessStatusCode)return Redirect($"/Platform/Tenants/{tenantId}?scopeError=denied");
        using var document=JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));var data=document.RootElement.TryGetProperty("data",out var nested)?nested:document.RootElement;var token=data.GetProperty("accessToken").GetString();if(string.IsNullOrWhiteSpace(token))return Redirect($"/Platform/Tenants/{tenantId}?scopeError=invalid");
        if(Request.Cookies.TryGetValue("BarberSync.AccessToken",out var original)&&!string.IsNullOrWhiteSpace(original))Response.Cookies.Append("BarberSync.PlatformToken",original,SecureCookie());
        Response.Cookies.Append("BarberSync.AccessToken",token,SecureCookie());await SignInToken(token);return Redirect("/Admin/Dashboard");
    }

    [HttpPost("Scope/Exit"),ValidateAntiForgeryToken]
    public async Task<IActionResult> ExitScope(CancellationToken ct)
    {
        if(User.FindFirst("scope_session_id")?.Value is { } value&&Guid.TryParse(value,out var scopeId)){var client=AuthenticatedClient();await client.DeleteAsync($"/api/platform/scope/{scopeId}",ct);}
        if(Request.Cookies.TryGetValue("BarberSync.PlatformToken",out var original)&&!string.IsNullOrWhiteSpace(original)){Response.Cookies.Append("BarberSync.AccessToken",original,SecureCookie());Response.Cookies.Delete("BarberSync.PlatformToken");await SignInToken(original);}
        return Redirect("/Platform");
    }

    private IActionResult Section(string title,string description){ViewData["Title"]=title;ViewData["Description"]=description;return View("Section");}
    private async Task<IActionResult> ForwardGet(string path,CancellationToken ct){try{var response=await AuthenticatedClient().GetAsync(path,ct);var body=await response.Content.ReadAsStringAsync(ct);return new ContentResult{StatusCode=(int)response.StatusCode,ContentType=response.Content.Headers.ContentType?.ToString()??"application/json",Content=body};}catch(HttpRequestException){return StatusCode(503,new{message="API da plataforma indisponível.",traceId=HttpContext.TraceIdentifier});}}
    private async Task<IActionResult> ForwardJson(HttpMethod method,string path,JsonElement body,CancellationToken ct){try{using var request=new HttpRequestMessage(method,path){Content=new StringContent(body.GetRawText(),Encoding.UTF8,"application/json")};var response=await AuthenticatedClient().SendAsync(request,ct);var content=await response.Content.ReadAsStringAsync(ct);return new ContentResult{StatusCode=(int)response.StatusCode,ContentType=response.Content.Headers.ContentType?.ToString()??"application/json",Content=content};}catch(HttpRequestException){return StatusCode(503,new{message="API da plataforma indisponível.",traceId=HttpContext.TraceIdentifier});}}
    private HttpClient AuthenticatedClient(){var client=clients.CreateClient("BarberSyncApi");if(Request.Cookies.TryGetValue("BarberSync.AccessToken",out var token))client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",token);return client;}
    private async Task SignInToken(string token){var claims=ReadClaims(token).ToList();await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme,ClaimTypes.Email,ClaimTypes.Role)),new AuthenticationProperties{IsPersistent=false,ExpiresUtc=DateTimeOffset.UtcNow.AddMinutes(15)});}
    private CookieOptions SecureCookie()=>new(){HttpOnly=true,Secure=Request.IsHttps,SameSite=SameSiteMode.Strict,Expires=DateTimeOffset.UtcNow.AddMinutes(20)};
    private static IEnumerable<Claim> ReadClaims(string jwt){var part=jwt.Split('.')[1].Replace('-','+').Replace('_','/');part=part.PadRight(part.Length+((4-part.Length%4)%4),'=');using var document=JsonDocument.Parse(Convert.FromBase64String(part));foreach(var property in document.RootElement.EnumerateObject()){var type=property.NameEquals("roles")?ClaimTypes.Role:property.NameEquals("email")?ClaimTypes.Email:property.Name;if(property.Value.ValueKind==JsonValueKind.Array){foreach(var value in property.Value.EnumerateArray())yield return new Claim(type,value.GetString()??string.Empty);}else if(property.Value.ValueKind==JsonValueKind.String)yield return new Claim(type,property.Value.GetString()!);}}
}
