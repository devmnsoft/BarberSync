using BarberSync.Application.DTOs.Ai;
using BarberSync.Application.Services.Ai;

namespace BarberSync.Tests;

public class CopilotServiceTests
{
    [Fact]
    public void Ask_Should_Create_Answer_And_Suggestions()
    {
        var service = new CopilotService(new MockAiProvider());
        var tenantId = Guid.NewGuid();

        var response = service.Ask(new CopilotAskRequestDto(tenantId, null, "Como foi o faturamento de hoje?"));

        Assert.NotEqual(Guid.Empty, response.ConversationId);
        Assert.Contains("Resumo do dia", response.Answer.Content);
        Assert.NotEmpty(response.Suggestions);
    }
}
