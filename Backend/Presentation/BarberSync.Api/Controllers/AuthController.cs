using BarberSync.Application.Abstractions;
using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ITokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public ActionResult<LoginResponseDto> Login([FromBody] LoginRequestDto request)
    {
        var token = tokenService.Generate(Guid.NewGuid().ToString(), request.Email, "Admin");
        return Ok(new LoginResponseDto(token));
    }
}
