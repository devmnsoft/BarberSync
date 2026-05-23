using BarberSync.Application.DTOs;
using BarberSync.Application.Services.Hr;

namespace BarberSync.Tests;

public class HrProfessionalServiceTests
{
    [Fact]
    public void Should_Create_And_List_Professional_By_Tenant()
    {
        var service = new HrProfessionalService();
        var tenant = Guid.NewGuid();

        var created = service.Create(new CreateHrProfessionalRequest(
            tenant,
            Guid.NewGuid(),
            "Ana Souza",
            EmploymentType.Clt,
            new DateOnly(2026, 5, 1),
            "Senior",
            45,
            25000,
            44,
            ["Colorimetria"],
            [Guid.NewGuid()],
            ["RG", "CPF"]));

        var list = service.GetAll(tenant);

        Assert.Single(list);
        Assert.Equal(created.Id, list.First().Id);
        Assert.Equal("Ativo", created.OperationalStatus);
    }
}
