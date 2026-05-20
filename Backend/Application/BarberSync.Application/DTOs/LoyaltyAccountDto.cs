namespace BarberSync.Application.DTOs;

public class LoyaltyAccountDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClientId { get; set; }
    public int PointsBalance { get; set; }
    public decimal CashbackBalance { get; set; }
    public int TierLevel { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
