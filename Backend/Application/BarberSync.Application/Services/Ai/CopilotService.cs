using BarberSync.Application.Abstractions.Ai;
using BarberSync.Application.DTOs.Ai;

namespace BarberSync.Application.Services.Ai;

public sealed class CopilotService(IAiProvider aiProvider) : ICopilotService
{
    private static readonly List<CopilotConversationDto> Conversations = [];
    private static readonly List<CopilotMessageDto> Messages = [];
    private static readonly List<CopilotSuggestionDto> Suggestions = [];
    private static readonly List<CopilotActionDto> Actions = [];
    private static readonly List<CopilotFeedbackDto> Feedbacks = [];

    public IReadOnlyCollection<CopilotConversationDto> GetConversations(Guid tenantId) =>
        Conversations.Where(c => c.TenantId == tenantId).OrderByDescending(c => c.CreatedAtUtc).ToArray();

    public IReadOnlyCollection<CopilotMessageDto> GetMessages(Guid conversationId) =>
        Messages.Where(m => m.ConversationId == conversationId).OrderBy(m => m.CreatedAtUtc).ToArray();

    public CopilotAskResponseDto Ask(CopilotAskRequestDto request)
    {
        var conversationId = request.ConversationId ?? Guid.NewGuid();
        if (request.ConversationId is null)
        {
            Conversations.Add(new CopilotConversationDto(conversationId, request.TenantId, "Conversa Copilot", DateTime.UtcNow));
        }

        Messages.Add(new CopilotMessageDto(Guid.NewGuid(), conversationId, "user", request.Question, DateTime.UtcNow));
        var answerText = aiProvider.GenerateAnswer(request.Question);
        var answer = new CopilotMessageDto(Guid.NewGuid(), conversationId, "assistant", answerText, DateTime.UtcNow);
        Messages.Add(answer);

        var generatedSuggestions = new[]
        {
            new CopilotSuggestionDto(Guid.NewGuid(), request.TenantId, "sales", "Sugestão inteligente", "Ofereça combo corte + barba nos horários ociosos.", "high", DateTime.UtcNow),
            new CopilotSuggestionDto(Guid.NewGuid(), request.TenantId, "retention", "Ação recomendada", "Contate clientes VIP sem retorno há 30 dias.", "medium", DateTime.UtcNow)
        };

        Suggestions.AddRange(generatedSuggestions);
        return new CopilotAskResponseDto(conversationId, answer, generatedSuggestions);
    }

    public IReadOnlyCollection<CopilotSuggestionDto> GetSuggestions(Guid tenantId) =>
        Suggestions.Where(s => s.TenantId == tenantId).OrderByDescending(s => s.CreatedAtUtc).Take(20).ToArray();

    public CopilotActionDto CreateAction(CopilotActionDto action)
    {
        var value = action with { Id = Guid.NewGuid(), CreatedAtUtc = DateTime.UtcNow };
        Actions.Add(value);
        return value;
    }

    public CopilotFeedbackDto SubmitFeedback(CopilotFeedbackDto feedback)
    {
        var value = feedback with { Id = Guid.NewGuid(), CreatedAtUtc = DateTime.UtcNow };
        Feedbacks.Add(value);
        return value;
    }
}

public sealed class MockAiProvider : IAiProvider
{
    public string GenerateAnswer(string prompt)
    {
        return $"Resumo do dia: faturamento estável. Pergunta recebida: {prompt}";
    }
}
