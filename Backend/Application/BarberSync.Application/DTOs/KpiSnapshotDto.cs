namespace BarberSync.Application.DTOs;

public class KpiSnapshotDto
{
    public DateTime SnapshotAtUtc { get; set; } = DateTime.UtcNow;
    public decimal Revenue { get; set; }
    public decimal Profit { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal SatisfactionScore { get; set; }
    public int CriticalStockItems { get; set; }
    public int OverloadedProfessionals { get; set; }
}
