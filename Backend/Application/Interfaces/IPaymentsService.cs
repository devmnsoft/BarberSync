using BarberSync.Application.DTOs;

namespace BarberSync.Application.Interfaces;

public interface IPaymentsService
{
    Task<(IEnumerable<PaymentDto> Items, int Total)> GetAllAsync(int page, int pageSize, string? search, string? sortBy, string? sortDir);
    Task<PaymentDto?> GetByIdAsync(Guid id);
    Task<PaymentDto> CreateAsync(CreatePaymentDto dto);
    Task<PaymentDto?> UpdateAsync(Guid id, UpdatePaymentDto dto);
    Task<bool> DeleteAsync(Guid id);
}
