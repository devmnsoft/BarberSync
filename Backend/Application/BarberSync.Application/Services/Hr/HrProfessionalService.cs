using BarberSync.Application.Abstractions.Hr;
using BarberSync.Application.DTOs;

namespace BarberSync.Application.Services.Hr;

public sealed class HrProfessionalService : IHrProfessionalService
{
    private static readonly List<HrProfessionalDto> Professionals = [];

    public IReadOnlyCollection<HrProfessionalDto> GetAll(Guid tenantId) =>
        Professionals.Where(p => p.TenantId == tenantId).OrderBy(p => p.FullName).ToArray();

    public HrProfessionalDto Create(CreateHrProfessionalRequest request)
    {
        var value = new HrProfessionalDto(
            Guid.NewGuid(),
            request.TenantId,
            request.BranchId,
            request.FullName,
            request.EmploymentType,
            request.AdmissionDate,
            null,
            request.TechnicalLevel,
            request.DefaultCommissionPercent,
            request.MonthlyGoal,
            request.WeeklyWorkloadHours,
            "Ativo",
            request.Specialties ?? [],
            request.AuthorizedServiceIds ?? [],
            request.RequiredDocuments ?? [],
            DateTime.UtcNow);

        Professionals.Add(value);
        return value;
    }

    public HrProfessionalDto? GetById(Guid id, Guid tenantId) =>
        Professionals.FirstOrDefault(p => p.Id == id && p.TenantId == tenantId);
}

