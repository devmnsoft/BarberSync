namespace BarberSync.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public char Status { get; protected set; } = 'A';

    public void Disable() => Status = 'I';
}
