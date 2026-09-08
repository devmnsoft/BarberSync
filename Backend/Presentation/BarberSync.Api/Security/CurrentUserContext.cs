using BarberSync.Application.Abstractions;
using System.Security.Claims;

namespace BarberSync.Api.Security;

public sealed class CurrentUserContext(IHttpContextAccessor accessor) : ICurrentUserContext
{
    private ClaimsPrincipal User => accessor.HttpContext?.User ?? throw new UnauthorizedAccessException();
    public Guid UserId => Required("sub");
    public Guid TenantId => Required("tenant_id");
    public Guid BranchId => Required("branch_id");
    public bool IsPlatformUser => string.Equals(User.FindFirstValue("platform_admin"), "true", StringComparison.OrdinalIgnoreCase);
    public Guid? ActorUserId => Optional("actor_user_id");
    public Guid? ScopeSessionId => Optional("scope_session_id");
    public IReadOnlySet<string> Roles => Values("roles");
    public IReadOnlySet<string> Permissions => Values("permissions");
    private Guid Required(string type) => Guid.TryParse(User.FindFirstValue(type), out var id) ? id : throw new UnauthorizedAccessException($"Claim {type} ausente.");
    private Guid? Optional(string type) => Guid.TryParse(User.FindFirstValue(type), out var id) ? id : null;
    private IReadOnlySet<string> Values(string type) => User.FindAll(type).Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
}
