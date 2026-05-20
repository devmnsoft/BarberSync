using BarberSync.Application.DTOs;

namespace BarberSync.Application.Interfaces;

public interface IAppointmentsService
{
    Task<(IEnumerable<AppointmentDto> Items, int Total)> GetAllAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir);
    Task<AppointmentDto?> GetByIdAsync(Guid id);
    Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto);
    Task<AppointmentDto?> UpdateAsync(Guid id, UpdateAppointmentDto dto);
    Task<bool> DeleteAsync(Guid id);
}
