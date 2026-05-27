namespace BarberSync.Api.Models.Kiosk;

public class KioskServiceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationMinutes { get; set; }
    public string Icon { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public bool IsDemo { get; set; }
}

public class KioskProfessionalDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Specialty { get; set; } = "Barbearia";
    public decimal Rating { get; set; } = 4.9m;
    public string Status { get; set; } = "Disponível";
    public int EstimatedWaitMinutes { get; set; }
    public string AvatarInitials { get; set; } = "BS";
}
