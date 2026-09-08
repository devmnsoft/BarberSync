using BarberSync.Application.Abstractions;
using BarberSync.Application.Abstractions.Saas;
using BarberSync.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BarberSync.Api.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireModuleAttribute : TypeFilterAttribute
{
    public RequireModuleAttribute(string moduleKey, string? permission = null, string? featureKey = null, string? quotaKey = null)
        : base(typeof(RequireModuleFilter))
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleKey);
        Arguments = [moduleKey, permission!, featureKey!, quotaKey!];
    }
}

public sealed class RequireModuleFilter(
    ICurrentUserContext currentUser,
    IModuleEntitlementService entitlements,
    string moduleKey,
    string? permission,
    string? featureKey,
    string? quotaKey) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        ModuleAccessDecision decision;
        try
        {
            decision = await entitlements.DecideAsync(new(currentUser.UserId,currentUser.TenantId,currentUser.BranchId,moduleKey,permission,featureKey,quotaKey),context.HttpContext.RequestAborted);
        }
        catch (UnauthorizedAccessException)
        {
            context.Result = new UnauthorizedObjectResult(new { code="UNAUTHENTICATED",message="Autenticação necessária.",traceId=context.HttpContext.TraceIdentifier });
            return;
        }
        if (decision.Allowed) return;
        var owner=currentUser.Roles.Any(role=>role.Equals("Owner",StringComparison.OrdinalIgnoreCase)||role.Equals("Admin",StringComparison.OrdinalIgnoreCase));
        var message=decision.ReasonCode=="MODULE_NOT_ENTITLED"&&owner
            ? "Este módulo não está incluído na assinatura atual."
            : decision.ReasonCode=="TENANT_INACTIVE" ? "A conta está temporariamente indisponível."
            : "Você não possui acesso a esta funcionalidade.";
        context.Result=new ObjectResult(new{code=decision.ReasonCode,message,moduleKey=decision.ModuleKey,traceId=context.HttpContext.TraceIdentifier}){StatusCode=StatusCodes.Status403Forbidden};
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePlatformPermissionAttribute : TypeFilterAttribute
{
    public RequirePlatformPermissionAttribute(string permission) : base(typeof(RequirePlatformPermissionFilter)) => Arguments=[permission];
}

public sealed class RequirePlatformPermissionFilter(ICurrentUserContext currentUser,string permission) : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (currentUser.IsPlatformUser && (currentUser.Roles.Contains("SuperAdmin") || currentUser.Permissions.Contains(permission))) return;
        context.Result=new ObjectResult(new{code="PLATFORM_ACCESS_DENIED",message="Acesso restrito à administração da plataforma.",traceId=context.HttpContext.TraceIdentifier}){StatusCode=StatusCodes.Status403Forbidden};
    }
}
