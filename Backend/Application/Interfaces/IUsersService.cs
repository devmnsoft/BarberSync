using BarberSync.Application.DTOs;

namespace BarberSync.Application.Interfaces;

public interface IUsersService
{
    Task<(IEnumerable<UserDto> Items, int Total)> GetAllAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir);
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task<UserDto?> UpdateAsync(Guid id, UpdateUserDto dto);
    Task<bool> DeleteAsync(Guid id);
}
