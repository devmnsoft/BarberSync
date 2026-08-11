using BarberSync.Api.Models;
using BarberSync.Application.Abstractions;
using BarberSync.Application.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService, IValidator<LoginRequestDto> validator) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Verifique os dados informados.", validation.Errors.Select(e => e.ErrorMessage), HttpContext.TraceIdentifier));
        var result = await authService.LoginAsync(request, IpAddress(), HttpContext.TraceIdentifier, cancellationToken);
        return result is null
            ? Unauthorized(ApiResponse<object>.Fail("Credenciais inválidas.", traceId: HttpContext.TraceIdentifier))
            : Ok(ApiResponse<LoginResponseDto>.Ok(result, "Login realizado com sucesso.", HttpContext.TraceIdentifier));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshAsync(request.RefreshToken, IpAddress(), HttpContext.TraceIdentifier, cancellationToken);
        return result is null
            ? Unauthorized(ApiResponse<object>.Fail("Refresh token inválido ou expirado.", traceId: HttpContext.TraceIdentifier))
            : Ok(ApiResponse<LoginResponseDto>.Ok(result, "Token renovado.", HttpContext.TraceIdentifier));
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequestDto request, CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(request.RefreshToken, IpAddress(), HttpContext.TraceIdentifier, cancellationToken);
        return NoContent();
    }

    private string? IpAddress() => HttpContext.Connection.RemoteIpAddress?.ToString();
}
