using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace BarberSync.Api.Controllers;

[ApiController]
[Route("api/loyalty")]
public class LoyaltyController : ControllerBase
{
    private static readonly List<LoyaltyAccountDto> Accounts = new();

    [HttpPost("accrue")]
    public ActionResult<LoyaltyAccountDto> Accrue([FromBody] LoyaltyAccountDto dto)
    {
        dto.UpdatedAtUtc = DateTime.UtcNow;
        dto.PointsBalance += 20;
        dto.CashbackBalance += 5m;
        Accounts.Add(dto);
        return Ok(dto);
    }

    [HttpGet("accounts")]
    public ActionResult<IEnumerable<LoyaltyAccountDto>> GetAccounts()
    {
        IEnumerable<LoyaltyAccountDto> data = Accounts.Count == 0
            ? [new LoyaltyAccountDto { ClientId = Guid.Parse("11111111-2222-3333-4444-555555555555"), PointsBalance = 1280, CashbackBalance = 38.50m, TierLevel = 3, UpdatedAtUtc = DateTime.UtcNow }]
            : Accounts;
        return Ok(data);
    }

    [HttpGet("summary")]
    public IActionResult Summary() => Ok(new { success = true, message = "Fidelidade carregada com sucesso.", data = new { totalCashback = 18420m, activeMembers = 132, expiringCashback = 2140m, balance = 18420m, cashbackMonth = 2340m }, isDemo = true });
}
