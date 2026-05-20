namespace BarberSync.Domain.Entities;

public class Payment : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
}
