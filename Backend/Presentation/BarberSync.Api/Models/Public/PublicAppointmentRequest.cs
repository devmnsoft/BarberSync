namespace BarberSync.Api.Models.Public;

public sealed record PublicAppointmentRequest(
    string ClientName,
    string Phone,
    string? Email,
    Guid ServiceId,
    Guid? ProfessionalId,
    DateTimeOffset ScheduledAt,
    string? Notes);
