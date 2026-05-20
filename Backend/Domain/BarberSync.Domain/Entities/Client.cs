namespace BarberSync.Domain.Entities;

public class Client : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
}
