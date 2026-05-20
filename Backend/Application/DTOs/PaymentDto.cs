namespace BarberSync.Application.DTOs;

public record PaymentDto(Guid Id, string Name, bool IsActive);
public record CreatePaymentDto(string Name);
public record UpdatePaymentDto(string Name, bool IsActive);
