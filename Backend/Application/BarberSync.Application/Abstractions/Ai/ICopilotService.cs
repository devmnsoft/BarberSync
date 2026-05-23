using BarberSync.Application.DTOs.Ai;

namespace BarberSync.Application.Abstractions.Ai;

public interface ICopilotService
{
    IReadOnlyCollection<CopilotConversationDto> GetConversations(Guid tenantId);
    IReadOnlyCollection<CopilotMessageDto> GetMessages(Guid conversationId);
    CopilotAskResponseDto Ask(CopilotAskRequestDto request);
    IReadOnlyCollection<CopilotSuggestionDto> GetSuggestions(Guid tenantId);
    CopilotActionDto CreateAction(CopilotActionDto action);
    CopilotFeedbackDto SubmitFeedback(CopilotFeedbackDto feedback);
}

public interface IAiProvider
{
    string GenerateAnswer(string prompt);
}
