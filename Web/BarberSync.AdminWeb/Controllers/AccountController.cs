using System.Text;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.AdminWeb.Controllers;

public class AccountController(
    IHttpClientFactory httpClientFactory,
    IWebHostEnvironment environment,
    IConfiguration configuration) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
        ViewData["ShowDiagnostics"] = environment.IsDevelopment();
        ViewData["AllowFirstAdmin"] = configuration.GetValue<bool>("AdminSetup:AllowFirstAdministrator");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(Failure("Informe e-mail e senha."));

        using var content = new StringContent(
            JsonSerializer.Serialize(new { request.Email, request.Password }),
            Encoding.UTF8,
            "application/json");
        try
        {
            var response = await httpClientFactory.CreateClient("BarberSyncApi").PostAsync("/api/auth/login", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var message = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized or System.Net.HttpStatusCode.Forbidden => "E-mail ou senha inválidos.",
                    System.Net.HttpStatusCode.ServiceUnavailable => "O serviço de autenticação não conseguiu acessar o banco de dados. Contate o administrador.",
                    _ when (int)response.StatusCode >= 500 => "O serviço de autenticação está indisponível no momento.",
                    _ => "Não foi possível autenticar com os dados informados."
                };
                return StatusCode((int)response.StatusCode, Failure(message));
            }

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var root = document.RootElement;
            var data = root.TryGetProperty("data", out var nested) ? nested : root;
            if (!data.TryGetProperty("accessToken", out var token) || string.IsNullOrWhiteSpace(token.GetString()) ||
                !data.TryGetProperty("refreshToken", out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken.GetString()))
                return StatusCode(StatusCodes.Status502BadGateway, Failure("A resposta do serviço de autenticação é inválida."));

            var expires = data.TryGetProperty("expiresAt", out var expiresAt) && expiresAt.TryGetDateTimeOffset(out var parsedExpiry)
                ? parsedExpiry : DateTimeOffset.UtcNow.AddMinutes(15);
            var claims = ReadClaims(token.GetString()!);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Email, ClaimTypes.Role)),
                new AuthenticationProperties { IsPersistent = false, ExpiresUtc = expires, AllowRefresh = false });
            var cookieOptions = new CookieOptions { HttpOnly = true, Secure = Request.IsHttps, SameSite = SameSiteMode.Strict, Expires = expires };
            Response.Cookies.Append("BarberSync.AccessToken", token.GetString()!, cookieOptions);
            Response.Cookies.Append("BarberSync.RefreshToken", refreshToken.GetString()!, new CookieOptions { HttpOnly = true, Secure = Request.IsHttps, SameSite = SameSiteMode.Strict });

            var redirectUrl = Url.IsLocalUrl(request.ReturnUrl) ? request.ReturnUrl : "/Admin/Dashboard";
            return Ok(new { redirectUrl });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, Failure("Não foi possível acessar a API de autenticação. Tente novamente em instantes."));
        }
        catch (Exception exception) when (exception is JsonException or FormatException or InvalidOperationException)
        {
            return StatusCode(StatusCodes.Status502BadGateway, Failure("A API retornou uma resposta de autenticação inválida."));
        }
    }

    [HttpGet]
    public IActionResult ForgotPassword() => Content("Solicite ao administrador da sua unidade a redefinição segura da senha.", "text/plain", Encoding.UTF8);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue("BarberSync.RefreshToken", out var refreshToken) && !string.IsNullOrWhiteSpace(refreshToken))
        {
            using var content = new StringContent(JsonSerializer.Serialize(new { refreshToken }), Encoding.UTF8, "application/json");
            try { await httpClientFactory.CreateClient("BarberSyncApi").PostAsync("/api/auth/logout", content, cancellationToken); }
            catch (HttpRequestException) { /* Cookies are still invalidated locally when the API is unavailable. */ }
        }
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        Response.Cookies.Delete("BarberSync.AccessToken");
        Response.Cookies.Delete("BarberSync.RefreshToken");
        return RedirectToAction(nameof(Login));
    }

    private static IEnumerable<Claim> ReadClaims(string jwt)
    {
        var segments = jwt.Split('.');
        if (segments.Length != 3) throw new InvalidOperationException("Token de acesso inválido.");
        var payload = segments[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + ((4 - payload.Length % 4) % 4), '=');
        using var document = JsonDocument.Parse(Convert.FromBase64String(payload));
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.NameEquals("roles") || property.NameEquals("permissions"))
            {
                var claimType = property.NameEquals("roles") ? ClaimTypes.Role : "permissions";
                if (property.Value.ValueKind == JsonValueKind.Array)
                    foreach (var value in property.Value.EnumerateArray()) yield return new Claim(claimType, value.GetString() ?? string.Empty);
                else yield return new Claim(claimType, property.Value.GetString() ?? string.Empty);
            }
            else if (property.Value.ValueKind == JsonValueKind.String)
                yield return new Claim(property.NameEquals("email") ? ClaimTypes.Email : property.Name, property.Value.GetString()!);
        }
    }

    private object Failure(string message) => new { message, traceId = HttpContext.TraceIdentifier };

    public sealed record LoginRequest(string Email, string Password, string? ReturnUrl = null);
}
