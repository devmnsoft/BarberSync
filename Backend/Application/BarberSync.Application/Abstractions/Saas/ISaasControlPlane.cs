using BarberSync.Application.DTOs;

namespace BarberSync.Application.Abstractions.Saas;

public interface IModuleEntitlementService
{
    Task<ModuleAccessDecision> DecideAsync(ModuleAccessRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, ModuleAccessDecision>> GetEffectiveAccessAsync(
        Guid userId, Guid tenantId, Guid branchId, CancellationToken cancellationToken = default);
    void Invalidate(Guid tenantId, Guid? userId = null, Guid? branchId = null);
}

public interface IEffectiveAccessCache
{
    bool TryGet(string key, out ModuleAccessDecision decision);
    void Set(string key, ModuleAccessDecision decision, TimeSpan lifetime);
    void Invalidate(Guid tenantId, Guid? userId, Guid? branchId);
}

public interface ISaasModuleRepository
{
    Task<IReadOnlyList<SaasModuleListItemDto>> ListAsync(CancellationToken cancellationToken);
    Task<SaasModuleListItemDto?> FindAsync(Guid id, CancellationToken cancellationToken);
    Task<Guid> CreateAsync(CreateSaasModuleRequest request, Guid actorUserId, string correlationId, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Guid id, UpdateSaasModuleRequest request, Guid actorUserId, string correlationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SaasModulePriceDto>> ListPricesAsync(Guid? moduleId, CancellationToken cancellationToken);
    Task<Guid> UpsertPriceAsync(Guid? id, UpsertSaasModulePriceRequest request, Guid actorUserId, string correlationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenantModuleContractDto>> ListContractsAsync(Guid? tenantId, Guid? moduleId, string? status, CancellationToken cancellationToken);
    Task<Guid> CreateContractAsync(CreateTenantModuleContractRequest request, Guid actorUserId, string correlationId, CancellationToken cancellationToken);
    Task<bool> ChangeContractStatusAsync(Guid id, ChangeModuleContractStatusRequest request, Guid actorUserId, string correlationId, CancellationToken cancellationToken);
}

public interface IPlatformTenantRepository
{
    Task<PlatformDashboardDto> DashboardAsync(DateOnly from, DateOnly until, CancellationToken cancellationToken);
    Task<PagedResult<PlatformTenantListItemDto>> ListAsync(PlatformTenantQuery query, CancellationToken cancellationToken);
    Task<PlatformTenantDetailDto?> DetailAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<PagedResult<PlatformTenantUserDto>> UsersAsync(Guid tenantId, int page, int pageSize, CancellationToken cancellationToken);
    Task<IReadOnlyList<PlatformTenantRoleDto>> RolesAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PlatformTenantBranchDto>> BranchesAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PlatformTenantUsageDto>> UsageAsync(Guid tenantId, DateOnly from, DateOnly until, CancellationToken cancellationToken);
    Task<IReadOnlyList<TenantModuleContractDto>> ModulesAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PlatformAuditDto>> AuditAsync(Guid? tenantId, int pageSize, CancellationToken cancellationToken);
}

public interface IPlatformScopeService
{
    Task<PlatformScopeTokenDto?> StartAsync(Guid actorUserId, StartPlatformScopeRequest request, string? ipAddress, string correlationId, CancellationToken cancellationToken);
    Task EndAsync(Guid actorUserId, Guid scopeSessionId, string correlationId, CancellationToken cancellationToken);
}
