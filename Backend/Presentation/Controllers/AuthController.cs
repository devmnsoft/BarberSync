using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest req)
        => Ok(new { req.Email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password) });

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest req)
        => Ok(new { token = "jwt-placeholder", req.Email });
}

public record RegisterRequest(string Email, string Password);
public record LoginRequest(string Email, string Password);
