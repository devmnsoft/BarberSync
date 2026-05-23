using BarberSync.Application.DTOs;

namespace BarberSync.Application.Abstractions.Hr;

public interface IHrProfessionalService
{
    IReadOnlyCollection<HrProfessionalDto> GetAll(Guid tenantId);
    HrProfessionalDto Create(CreateHrProfessionalRequest request);
    HrProfessionalDto? GetById(Guid id, Guid tenantId);
}

