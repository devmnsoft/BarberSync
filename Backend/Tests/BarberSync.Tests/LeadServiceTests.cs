using BarberSync.Application.DTOs.Commercial;
using BarberSync.Application.Services.Commercial;

namespace BarberSync.Tests;

public class LeadServiceTests
{
    [Fact]
    public void Nao_Deve_Permitir_Lead_Duplicado_Por_Telefone()
    {
        var service = new CrmLeadService();
        var tenantId = Guid.NewGuid();
        service.CreateLead(new(tenantId, null, "João", "11999990000", "joao@x.com", "TOTEM", "HOT", null));

        var ex = Assert.Throws<InvalidOperationException>(() =>
            service.CreateLead(new(tenantId, null, "Maria", "11999990000", "maria@x.com", "WHATSAPP", "WARM", null)));

        Assert.Equal("Já existe um lead com este telefone ou e-mail.", ex.Message);
    }

    [Fact]
    public void Lead_Convertido_Nao_Pode_Converter_Novamente()
    {
        var service = new CrmLeadService();
        var lead = service.CreateLead(new(Guid.NewGuid(), null, "Cliente", "11911112222", "c@x.com", "SITE", "HOT", null));
        service.Convert(lead.Id, Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => service.Convert(lead.Id, Guid.NewGuid()));
    }

    [Fact]
    public void Lead_Perdido_Exige_Motivo()
    {
        var service = new CrmLeadService();
        var lead = service.CreateLead(new(Guid.NewGuid(), null, "Lead", null, "lead@x.com", "SITE", "COLD", null));

        var ex = Assert.Throws<InvalidOperationException>(() => service.UpdateStatus(lead.Id, new("LOST", null)));
        Assert.Equal("Informe o motivo da perda.", ex.Message);
    }
}
