using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

public class AccountController(IHttpClientFactory httpClientFactory) : Controller
{
    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Informe e-mail e senha." });

        using var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        try
        {
            var response = await httpClientFactory.CreateClient("BarberSyncApi").PostAsync("/api/auth/login", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, new { message = "Não foi possível autenticar com os dados informados." });

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var root = document.RootElement;
            var data = root.TryGetProperty("data", out var nested) ? nested : root;
            if (data.TryGetProperty("token", out var token) && !string.IsNullOrWhiteSpace(token.GetString()))
                Response.Cookies.Append("BarberSync.AccessToken", token.GetString()!, new CookieOptions { HttpOnly = true, Secure = Request.IsHttps, SameSite = SameSiteMode.Lax });

            return Ok(new { redirectUrl = "/Admin/Dashboard" });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Serviço de autenticação temporariamente indisponível." });
        }
    }

    [HttpGet]
    public IActionResult ForgotPassword() => Content("Solicite ao administrador da sua unidade a redefinição segura da senha.", "text/plain", Encoding.UTF8);

    public sealed record LoginRequest(string Email, string Password);
}
