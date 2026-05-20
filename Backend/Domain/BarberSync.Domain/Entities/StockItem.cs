namespace BarberSync.Domain.Entities;

public class StockItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
}
