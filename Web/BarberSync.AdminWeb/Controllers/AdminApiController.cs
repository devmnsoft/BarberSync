using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

/// <summary>Same-origin gateway for the Admin. It never manufactures business data or success responses.</summary>
[ApiController]
[Route("AdminApi")]
public sealed class AdminApiController(IHttpClientFactory clients, IConfiguration configuration, ILogger<AdminApiController> logger) : ControllerBase
{
    [HttpGet("{**path}")]
    public Task<IActionResult> Get(string path, CancellationToken ct) => Forward(HttpMethod.Get, path, null, ct);

    [HttpPost("{**path}")]
    public Task<IActionResult> Post(string path, [FromBody] JsonElement? body, CancellationToken ct) => Forward(HttpMethod.Post, path, body, ct);

    [HttpPut("{**path}")]
    public Task<IActionResult> Put(string path, [FromBody] JsonElement? body, CancellationToken ct) => Forward(HttpMethod.Put, path, body, ct);

    [HttpPatch("{**path}")]
    public Task<IActionResult> Patch(string path, [FromBody] JsonElement? body, CancellationToken ct) => Forward(HttpMethod.Patch, path, body, ct);

    [HttpDelete("{**path}")]
    public Task<IActionResult> Delete(string path, CancellationToken ct) => Forward(HttpMethod.Delete, path, null, ct);

    private async Task<IActionResult> Forward(HttpMethod method, string path, JsonElement? body, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(method, BuildUrl(path, Request.QueryString.Value));
            var authorization = Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authorization))
                request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorization);
            else if (Request.Cookies.TryGetValue("BarberSync.AccessToken", out var accessToken) && !string.IsNullOrWhiteSpace(accessToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            if (body.HasValue && body.Value.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
                request.Content = new StringContent(body.Value.GetRawText(), Encoding.UTF8, "application/json");

            using var response = await clients.CreateClient("BarberSyncApi").SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            return new ContentResult
            {
                StatusCode = (int)response.StatusCode,
                Content = content,
                ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json"
            };
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "A API não respondeu a {Method} /api/{Path}.", method, path);
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Serviço temporariamente indisponível",
                detail: "Não foi possível concluir a operação. Tente novamente em alguns instantes.");
        }
    }

    private string BuildUrl(string path, string? query)
    {
        var root = (configuration["ApiSettings:BaseUrl"] ?? configuration["ApiBaseUrl"] ?? "http://localhost:5080").TrimEnd('/');
        var apiPath = path.Equals("api-health", StringComparison.OrdinalIgnoreCase) ? "health" : $"api/{path.TrimStart('/')}";
        return $"{root}/{apiPath}{query}";
    }
}
