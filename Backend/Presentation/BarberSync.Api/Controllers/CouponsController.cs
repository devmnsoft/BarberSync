using BarberSync.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/coupons")]
public class CouponsController : ControllerBase
{
    private static readonly List<CouponDto> Coupons =
    [
        new(Guid.Parse("33333333-3333-3333-3333-333333333333"), "VIP10", "Percentual", 10, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), 100, true),
        new(Guid.Parse("44444444-4444-4444-4444-444444444444"), "VOLTE20", "Valor", 20, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)), 50, true)
    ];

    [HttpGet]
    public IActionResult Get() => Ok(ApiResponse<IEnumerable<CouponDto>>.Ok(Coupons, "Cupons carregados com sucesso.", HttpContext.TraceIdentifier));

    [HttpPost]
    public IActionResult Create([FromBody] CreateCouponRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest(ApiResponse<object>.Fail("Código do cupom é obrigatório.", ["Informe o código do cupom."], HttpContext.TraceIdentifier));

        var coupon = new CouponDto(Guid.NewGuid(), request.Code.Trim().ToUpperInvariant(), request.Type?.Trim() ?? "Percentual", request.Value, request.ExpirationDate, request.Limit, true);
        Coupons.Add(coupon);
        return Ok(ApiResponse<CouponDto>.Ok(coupon, "Cupom criado com sucesso.", HttpContext.TraceIdentifier));
    }

    public sealed record CreateCouponRequest(string Code, string? Type, decimal Value, DateOnly ExpirationDate, int Limit);
    public sealed record CouponDto(Guid Id, string Code, string Type, decimal Value, DateOnly ExpirationDate, int Limit, bool Active);
}
