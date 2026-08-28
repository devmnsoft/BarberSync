using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
namespace BarberSync.PublicWeb.Controllers;
[Route("")]
public sealed class MarketingPublicController(IHttpClientFactory clients):Controller
{
 [HttpGet("p/{slug}")]public async Task<IActionResult>Landing(string slug,CancellationToken ct){var response=await clients.CreateClient("BarberSyncApi").GetAsync($"/api/marketing/public/landing/{Uri.EscapeDataString(slug)}",ct);if(response.StatusCode==HttpStatusCode.NotFound){ViewData["Unavailable"]=true;return View("Landing");}if(!response.IsSuccessStatusCode){ViewData["Unavailable"]=true;ViewData["TraceId"]=response.Headers.TryGetValues("x-trace-id",out var v)?v.FirstOrDefault():HttpContext.TraceIdentifier;return View("Landing");}var envelope=await response.Content.ReadFromJsonAsync<LandingEnvelope>(cancellationToken:ct);ViewData["Slug"]=slug;return View("Landing",envelope?.Data);}
 [HttpGet("go/{publicSlug}")]public async Task<IActionResult>Go(string publicSlug,CancellationToken ct){var response=await clients.CreateClient("BarberSyncApi").GetAsync($"/api/marketing/public/go/{Uri.EscapeDataString(publicSlug)}",ct);if(response.StatusCode==HttpStatusCode.NotFound){ViewData["Unavailable"]=true;return View("Landing");}return response.Headers.Location is{} location?Redirect(location.ToString()):StatusCode(502);}
 [HttpPost("api/marketing/public/track")]public async Task<IActionResult>Track([FromBody] object payload,CancellationToken ct){var response=await clients.CreateClient("BarberSyncApi").PostAsJsonAsync("/api/marketing/public/track",payload,ct);return StatusCode((int)response.StatusCode);}
 public sealed record LandingEnvelope(bool Success,LandingModel? Data);public sealed record LandingModel(string Slug,string Title,string? Subtitle,string? Body,string? CtaLabel,string CtaType,string? CtaUrl);
}
