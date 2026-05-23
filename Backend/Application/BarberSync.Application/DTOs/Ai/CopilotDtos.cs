namespace BarberSync.Application.DTOs.Ai;

public record CopilotConversationDto(Guid Id, Guid TenantId, string Title, DateTime CreatedAtUtc);
public record CopilotMessageDto(Guid Id, Guid ConversationId, string Role, string Content, DateTime CreatedAtUtc);
public record CopilotSuggestionDto(Guid Id, Guid TenantId, string Category, string Title, string Description, string Priority, DateTime CreatedAtUtc);
public record CopilotActionDto(Guid Id, Guid TenantId, string ActionType, string Description, string Status, DateTime CreatedAtUtc);
public record CopilotFeedbackDto(Guid Id, Guid SuggestionId, Guid TenantId, int Score, string? Comment, DateTime CreatedAtUtc);
public record CopilotAskRequestDto(Guid TenantId, Guid? ConversationId, string Question);
public record CopilotAskResponseDto(Guid ConversationId, CopilotMessageDto Answer, IReadOnlyCollection<CopilotSuggestionDto> Suggestions);
