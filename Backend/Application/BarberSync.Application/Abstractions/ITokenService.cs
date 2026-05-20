namespace BarberSync.Application.Abstractions;

public interface ITokenService
{
    string Generate(string userId, string email, string role);
}
