using BarberSync.Application.DTOs.Ai;
using BarberSync.Application.Abstractions.Ai;
using BarberSync.Application.Services.Ai;

namespace BarberSync.Tests;

public class CopilotServiceTests
{
    [Fact]
    public void Unconfigured_provider_should_return_safe_answer_without_suggestions()
    {
        var service = new CopilotService(new UnconfiguredAiProvider());
        var tenantId = Guid.NewGuid();

        var response = service.Ask(new CopilotAskRequestDto(tenantId, null, "Como foi o faturamento de hoje?"));

        Assert.NotEqual(Guid.Empty, response.ConversationId);
        Assert.Contains("não está configurado", response.Answer.Content);
        Assert.Empty(response.Suggestions);
    }

    [Fact]
    public void Test_provider_should_preserve_controlled_answer_without_inventing_suggestions()
    {
        var service = new CopilotService(new TestAiProvider("Resposta controlada para o teste."));

        var response = service.Ask(new CopilotAskRequestDto(Guid.NewGuid(), null, "Pergunta"));

        Assert.Equal("Resposta controlada para o teste.", response.Answer.Content);
        Assert.Empty(response.Suggestions);
    }

    [Fact]
    public void Provider_failure_should_not_expose_exception_or_stack_trace()
    {
        var service = new CopilotService(new TestAiProvider(exception: new InvalidOperationException("token-secreto")));

        var response = service.Ask(new CopilotAskRequestDto(Guid.NewGuid(), null, "Pergunta"));

        Assert.Contains("temporariamente indisponível", response.Answer.Content);
        Assert.DoesNotContain("token-secreto", response.Answer.Content);
        Assert.Empty(response.Suggestions);
    }

    private sealed class TestAiProvider(string? answer = null, Exception? exception = null) : IAiProvider
    {
        public string GenerateAnswer(string prompt) => exception is null ? answer! : throw exception;
    }
}
