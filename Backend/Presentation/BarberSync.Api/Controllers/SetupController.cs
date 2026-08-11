using BarberSync.Api.Models;
using BarberSync.Application.Abstractions;
using BarberSync.Application.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BarberSync.Api.Controllers;

[ApiController, Route("api/setup")]
public sealed class SetupController(IFirstAdminSetupService setupService, IValidator<FirstAdminRequestDto> validator) : ControllerBase
{
    [AllowAnonymous, HttpPost("first-admin"), EnableRateLimiting("login")]
    public async Task<IActionResult> FirstAdmin(FirstAdminRequestDto request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Verifique os dados informados.", validation.Errors.Select(x => x.ErrorMessage), HttpContext.TraceIdentifier));

        var result = await setupService.CreateAsync(request, HttpContext.TraceIdentifier, cancellationToken);
        return result.Created
            ? StatusCode(StatusCodes.Status201Created, ApiResponse<object>.Ok(new { }, result.Message, HttpContext.TraceIdentifier))
            : Conflict(ApiResponse<object>.Fail(result.Message, traceId: HttpContext.TraceIdentifier));
    }
}

