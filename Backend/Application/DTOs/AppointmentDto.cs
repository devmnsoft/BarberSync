namespace BarberSync.Application.DTOs;

public record AppointmentDto(Guid Id, string Name, bool IsActive);
public record CreateAppointmentDto(string Name);
public record UpdateAppointmentDto(string Name, bool IsActive);
