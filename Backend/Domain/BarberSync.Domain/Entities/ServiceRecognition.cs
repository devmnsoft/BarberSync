namespace BarberSync.Domain.Entities;

public class ServiceRecognition : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
}
