using BarberSync.Application.Abstractions;

namespace BarberSync.Infrastructure.Security;

public sealed class JwtTokenService : ITokenService
{
    public string Generate(string userId, string email, string role)
        => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{userId}:{email}:{role}"));
}
