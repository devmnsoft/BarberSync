using BarberSync.Api.Models;
using BarberSync.Application.Abstractions;
using BarberSync.Application.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ITokenService tokenService, IValidator<LoginRequestDto> validator, ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);
            var token = tokenService.Generate(Guid.NewGuid().ToString(), request.Email, "Admin");
            return Ok(ApiResponse<LoginResponseDto>.Ok(new LoginResponseDto(token), "Login realizado com sucesso.", HttpContext.TraceIdentifier));
        }
        catch (ValidationException ex)
        {
            logger.LogWarning(ex, "Erro de validação em login. TraceId={TraceId}", HttpContext.TraceIdentifier);
            return BadRequest(ApiResponse<object>.Fail("Verifique os dados informados.", ex.Errors.Select(x => x.ErrorMessage), HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro inesperado em login. TraceId={TraceId}", HttpContext.TraceIdentifier);
            return StatusCode(500, ApiResponse<object>.Fail("Ocorreu um erro inesperado. Tente novamente ou contate o suporte.", traceId: HttpContext.TraceIdentifier));
        }
    }
}
