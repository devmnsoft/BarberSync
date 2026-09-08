using BarberSync.Application.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BarberSync.Infrastructure.Security;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    public string Generate(AuthUser user)
    {
        var jwt = options.Value;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        if (user.TenantId != Guid.Empty) claims.Add(new("tenant_id", user.TenantId.ToString()));
        if (user.BranchId != Guid.Empty) claims.Add(new("branch_id", user.BranchId.ToString()));
        if (user.IsPlatformUser) claims.Add(new("platform_admin", "true"));
        if (user.ActorUserId.HasValue) claims.Add(new("actor_user_id", user.ActorUserId.Value.ToString()));
        if (user.ScopeSessionId.HasValue) claims.Add(new("scope_session_id", user.ScopeSessionId.Value.ToString()));
        if (!string.IsNullOrWhiteSpace(user.TenantName)) claims.Add(new("tenant_name", user.TenantName));
        if (!string.IsNullOrWhiteSpace(user.BranchName)) claims.Add(new("branch_name", user.BranchName));
        claims.AddRange(user.Roles.Select(role => new Claim("roles", role)));
        claims.AddRange(user.Permissions.Select(permission => new Claim("permissions", permission)));
        claims.AddRange((user.Modules ?? []).Select(module => new Claim("modules", module)));
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)), SecurityAlgorithms.HmacSha256);
        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            jwt.Issuer, jwt.Audience, claims, expires: DateTime.UtcNow.AddMinutes(jwt.AccessTokenMinutes),
            signingCredentials: credentials));
    }
}
