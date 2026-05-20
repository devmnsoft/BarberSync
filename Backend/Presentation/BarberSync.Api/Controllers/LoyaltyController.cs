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
    public ActionResult<IEnumerable<LoyaltyAccountDto>> GetAccounts() => Ok(Accounts);
}
