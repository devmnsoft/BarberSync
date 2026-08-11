using BarberSync.Application.DTOs;

namespace BarberSync.Application.Abstractions;

public interface IFirstAdminSetupService
{
    Task<FirstAdminSetupResult> CreateAsync(FirstAdminRequestDto request, string correlationId, CancellationToken cancellationToken);
}

public sealed record FirstAdminSetupResult(bool Created, string Message);

