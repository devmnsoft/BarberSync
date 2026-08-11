namespace BarberSync.Application.DTOs;

public sealed record FirstAdminRequestDto(
    string Email,
    string Password,
    string FullName,
    string TenantSlug,
    string BranchCode);

