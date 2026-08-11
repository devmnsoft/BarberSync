using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BarberSync.Api.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : TypeFilterAttribute
{
    public RequirePermissionAttribute(string permission) : base(typeof(RequirePermissionFilter)) => Arguments = [permission];
}

public sealed class RequirePermissionFilter(BarberSync.Application.Abstractions.ICurrentUserContext currentUser, string permission) : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!currentUser.Permissions.Contains(permission) &&
            !currentUser.Roles.Contains("Admin") &&
            !currentUser.Roles.Contains("Owner") &&
            !currentUser.Roles.Contains("SuperAdmin"))
            context.Result = new ObjectResult(new { message = "Permissão necessária.", permission }) { StatusCode = StatusCodes.Status403Forbidden };
    }
}
