using BarberSync.Api.Validators;
using BarberSync.Application.DTOs;

namespace BarberSync.Tests;

public sealed class FirstAdminRequestValidatorTests
{
    private readonly FirstAdminRequestValidator _validator = new();

    [Fact]
    public void AcceptsStrongFirstAdminRequest()
    {
        var result = _validator.Validate(new FirstAdminRequestDto(
            "owner@empresa.com.br", "SenhaForte#2026", "Proprietário", "minha-empresa", "MATRIZ"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("curta#A1")]
    [InlineData("semm maiuscula#2026")]
    [InlineData("SemNumeroEspecial")]
    public void RejectsWeakPassword(string password)
    {
        var result = _validator.Validate(new FirstAdminRequestDto(
            "owner@empresa.com.br", password, "Proprietário", "minha-empresa", "MATRIZ"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(FirstAdminRequestDto.Password));
    }
}
