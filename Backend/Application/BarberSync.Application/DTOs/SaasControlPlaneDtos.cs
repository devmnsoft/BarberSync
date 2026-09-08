namespace BarberSync.Application.DTOs;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, long Total)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(Total / (double)PageSize);
}

public sealed record ModuleAccessRequest(
    Guid UserId,
    Guid TenantId,
    Guid BranchId,
    string ModuleKey,
    string? Permission = null,
    string? FeatureKey = null,
    string? QuotaKey = null);

public sealed record ModuleAccessDecision(
    bool Allowed,
    string ModuleKey,
    string ReasonCode,
    string Reason,
    Guid TenantId,
    Guid UserId,
    string? SubscriptionStatus,
    string? ModuleContractStatus);

public sealed record SaasModuleListItemDto(
    Guid Id,string ModuleKey,string Name,string? Description,string Category,string Status,int DisplayOrder,
    string? IconKey,bool IsCore,bool IsSellable,decimal? CurrentMonthlyPrice,string Currency,int ActiveTenants,
    IReadOnlyList<string> Dependencies);

public sealed record CreateSaasModuleRequest(
    string ModuleKey,string Name,string? Description,string Category,string Status,int DisplayOrder,string? IconKey,bool IsCore,bool IsSellable);

public sealed record UpdateSaasModuleRequest(
    string Name,string? Description,string Category,string Status,int DisplayOrder,string? IconKey,bool IsCore,bool IsSellable);

public sealed record SaasModulePriceDto(
    Guid Id,Guid ModuleId,string ModuleKey,string ModuleName,string BillingCycle,string Currency,decimal Price,
    DateTimeOffset ValidFrom,DateTimeOffset? ValidUntil,string Status);

public sealed record UpsertSaasModulePriceRequest(
    Guid ModuleId,string BillingCycle,string Currency,decimal Price,DateTimeOffset ValidFrom,DateTimeOffset? ValidUntil,string Status);

public sealed record TenantModuleContractDto(
    Guid Id,Guid TenantId,string TenantName,Guid ModuleId,string ModuleKey,string ModuleName,string Status,string BillingCycle,
    decimal? ContractedPrice,string Currency,DateTimeOffset StartsAt,DateTimeOffset? TrialEndsAt,DateTimeOffset? EndsAt,
    DateTimeOffset? SuspendedAt,DateTimeOffset? CancelledAt);

public sealed record CreateTenantModuleContractRequest(
    Guid TenantId,Guid ModuleId,string Status,string BillingCycle,decimal? ContractedPrice,string Currency,
    DateTimeOffset StartsAt,DateTimeOffset? TrialEndsAt,DateTimeOffset? EndsAt,string Reason);

public sealed record ChangeModuleContractStatusRequest(string Status,string Reason);

public sealed record PlatformDashboardDto(
    long ActiveTenants,long TrialTenants,long SuspendedTenants,long CancelledTenants,long PastDueTenants,long NewTenants,
    long Branches,long Users,long Professionals,long Clients,long Kiosks,long MobileUsers,long ActiveIntegrations,
    decimal Mrr,decimal Arr,decimal AverageTicket,IReadOnlyList<PlatformPlanMetricDto> Plans,IReadOnlyList<PlatformModuleMetricDto> Modules);

public sealed record PlatformPlanMetricDto(Guid PlanId,string PlanName,long Tenants,decimal Revenue);
public sealed record PlatformModuleMetricDto(
    Guid ModuleId,string ModuleKey,string Name,long ContractedTenants,long TrialTenants,long ActiveTenants,long SuspendedTenants,
    long ActiveUsers,long Requests,long RelevantOperations,decimal AdoptionPercent,decimal GrowthPercent,decimal Revenue);

public sealed record PlatformTenantQuery(
    string? Search,string? Status,Guid? PlanId,Guid? ModuleId,DateOnly? CreatedFrom,DateOnly? CreatedUntil,
    bool? Trial,string? CommercialStatus,Guid? BranchId,int? MinUsers,int Page=1,int PageSize=25);

public sealed record PlatformTenantListItemDto(
    Guid Id,string Name,string Slug,string? MaskedDocument,string? InstitutionalEmail,string Status,string? PlanName,
    int Modules,int Users,int Branches,DateTimeOffset CreatedAt,DateTimeOffset? LastAccess);

public sealed record PlatformTenantDetailDto(
    Guid Id,string Name,string Slug,string? DocumentType,string? MaskedDocument,string? InstitutionalEmail,string Status,
    DateTimeOffset CreatedAt,string? PlanName,string? SubscriptionStatus,string? BillingCycle,DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,int Branches,int Users,int Professionals,int Clients,DateTimeOffset? LastAccess);

public sealed record PlatformTenantUserDto(
    Guid Id,string Name,string? MaskedCpf,string Email,string Status,string? BranchName,IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,DateTimeOffset? LastAccess);

public sealed record PlatformTenantRoleDto(Guid Id,string Name,string Code,bool IsSystem,long Users,IReadOnlyList<string> Permissions);
public sealed record PlatformTenantBranchDto(Guid Id,string Name,string? Code,string Status,long Users,long Professionals);
public sealed record PlatformTenantUsageDto(Guid ModuleId,string ModuleKey,string ModuleName,DateOnly Date,int ActiveUsers,long Requests,long RelevantOperations,decimal? UsageUnits);

public sealed record PlatformAuditDto(
    Guid Id,Guid? ActorUserId,Guid? TargetTenantId,Guid? TargetBranchId,Guid? TargetUserId,string Module,string Action,
    string Entity,Guid? EntityId,string? Reason,string? CorrelationId,DateTimeOffset CreatedAt);

public sealed record StartPlatformScopeRequest(Guid TenantId,Guid? BranchId,string Reason);
public sealed record PlatformScopeTokenDto(Guid ScopeSessionId,Guid TenantId,Guid BranchId,string AccessToken,DateTimeOffset ExpiresAt);
