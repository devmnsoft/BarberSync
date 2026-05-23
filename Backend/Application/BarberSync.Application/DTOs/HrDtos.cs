namespace BarberSync.Application.DTOs;

public enum EmploymentType
{
    Clt,
    Mei,
    Autonomo,
    Comissionado,
    Parceiro,
    Freelancer
}

public record HrProfessionalDto(
    Guid Id,
    Guid TenantId,
    Guid BranchId,
    string FullName,
    EmploymentType EmploymentType,
    DateOnly AdmissionDate,
    DateOnly? TerminationDate,
    string TechnicalLevel,
    decimal DefaultCommissionPercent,
    decimal MonthlyGoal,
    int WeeklyWorkloadHours,
    string OperationalStatus,
    IReadOnlyCollection<string> Specialties,
    IReadOnlyCollection<Guid> AuthorizedServiceIds,
    IReadOnlyCollection<string> RequiredDocuments,
    DateTime CreatedAtUtc);

public record CreateHrProfessionalRequest(
    Guid TenantId,
    Guid BranchId,
    string FullName,
    EmploymentType EmploymentType,
    DateOnly AdmissionDate,
    string TechnicalLevel,
    decimal DefaultCommissionPercent,
    decimal MonthlyGoal,
    int WeeklyWorkloadHours,
    IReadOnlyCollection<string>? Specialties,
    IReadOnlyCollection<Guid>? AuthorizedServiceIds,
    IReadOnlyCollection<string>? RequiredDocuments);

