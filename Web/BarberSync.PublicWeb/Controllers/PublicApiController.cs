using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.PublicWeb.Controllers;

[ApiController]
[Route("PublicApi")]
public sealed class PublicApiController(IHttpClientFactory clients, IConfiguration configuration, ILogger<PublicApiController> logger) : ControllerBase
{
    [HttpGet("services")]
    public Task<IActionResult> Services(CancellationToken ct) => ProxyAsync(HttpMethod.Get, "/api/public/services", null, ct);

    [HttpGet("professionals")]
    public Task<IActionResult> Professionals(CancellationToken ct) => ProxyAsync(HttpMethod.Get, "/api/public/professionals", null, ct);

    [HttpPost("appointments")]
    public Task<IActionResult> CreateAppointment([FromBody] PublicAppointmentRequest request, CancellationToken ct)
        => ProxyAsync(HttpMethod.Post, "/api/public/appointments", request, ct);

    private async Task<IActionResult> ProxyAsync(HttpMethod method, string path, object? body, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(method, BuildUrl(path));
            if (body is not null)
                request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, MediaTypeNames.Application.Json);

            using var response = await clients.CreateClient("BarberSyncApi").SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            var content = await response.Content.ReadAsStringAsync(ct);
            return new ContentResult
            {
                Content = string.IsNullOrWhiteSpace(content) ? ErrorJson("A API não retornou uma resposta válida.") : content,
                ContentType = response.Content.Headers.ContentType?.MediaType ?? MediaTypeNames.Application.Json,
                StatusCode = (int)response.StatusCode
            };
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "API operacional indisponível em {Path}.", path);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, Error("API operacional indisponível. Confirme se BarberSync.Api está rodando e se a URL configurada está correta."));
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            logger.LogError(ex, "Timeout da API operacional em {Path}.", path);
            return StatusCode(StatusCodes.Status504GatewayTimeout, Error("A operação demorou mais que o esperado. Tente novamente."));
        }
    }

    private string BuildUrl(string path) => $"{(configuration["ApiSettings:BaseUrl"] ?? configuration["ApiBaseUrl"] ?? "http://localhost:5080").TrimEnd('/')}/{path.TrimStart('/')}";
    private object Error(string message) => new { success = false, message, errors = Array.Empty<object>(), traceId = HttpContext.TraceIdentifier };
    private string ErrorJson(string message) => JsonSerializer.Serialize(Error(message), JsonOptions);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
}

public sealed record PublicAppointmentRequest(
    string ClientName,
    string Phone,
    string? Email,
    Guid ServiceId,
    Guid? ProfessionalId,
    DateTimeOffset ScheduledAt,
    string? Notes);
