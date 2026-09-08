using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

[Authorize,Route("Subscription")]
public sealed class SubscriptionController(IHttpClientFactory clients) : Controller
{
    [HttpGet("Modules")]
    public IActionResult Modules() => View();

    [HttpGet("Data/Modules")]
    public Task<IActionResult> ModuleData(CancellationToken cancellationToken) =>
        Forward(new HttpRequestMessage(HttpMethod.Get,"/api/governance/modules/catalog"),cancellationToken);

    [HttpPost("Modules/{moduleId:guid}/Request"),ValidateAntiForgeryToken]
    public Task<IActionResult> RequestModule(Guid moduleId,[FromForm]string billingCycle,[FromForm]string reason,CancellationToken cancellationToken)
    {
        var body=JsonSerializer.Serialize(new{billingCycle,reason});
        return Forward(new HttpRequestMessage(HttpMethod.Post,$"/api/governance/modules/{moduleId}/request")
        {
            Content=new StringContent(body,Encoding.UTF8,"application/json")
        },cancellationToken);
    }

    private async Task<IActionResult> Forward(HttpRequestMessage request,CancellationToken cancellationToken)
    {
        try
        {
            var client=clients.CreateClient("BarberSyncApi");
            if(Request.Cookies.TryGetValue("BarberSync.AccessToken",out var token))request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",token);
            using var response=await client.SendAsync(request,cancellationToken);
            var content=await response.Content.ReadAsStringAsync(cancellationToken);
            return new ContentResult{StatusCode=(int)response.StatusCode,ContentType=response.Content.Headers.ContentType?.ToString()??"application/json",Content=content};
        }
        catch(HttpRequestException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,new{message="Serviço de assinatura indisponível.",traceId=HttpContext.TraceIdentifier});
        }
    }
}
