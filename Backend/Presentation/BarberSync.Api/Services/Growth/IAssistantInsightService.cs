namespace BarberSync.Api.Services.Growth;

public interface IAssistantInsightService
{
    Task<IReadOnlyList<AssistantInsight>> GetDashboardAsync(Guid tenantId, Guid branchId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AssistantInsight>> GetClientAsync(Guid tenantId, Guid clientId, CancellationToken cancellationToken);
}

public sealed record AssistantActionSuggestion(string Label, string Url);
public sealed record AssistantInsightResponse(
    string Title, string Description, string Priority, string Reason,
    string RelatedModule, AssistantActionSuggestion SuggestedAction);

public sealed record AssistantInsight(string Type, string Priority, string Message, string? ActionUrl = null);

public interface IAssistantRepository
{
    Task<IReadOnlyList<AssistantInsightResponse>> GetOperationalInsightsAsync(Guid tenantId, Guid branchId, CancellationToken cancellationToken);
}

public interface IAssistantService
{
    Task<IReadOnlyList<AssistantInsightResponse>> GetInsightsAsync(Guid tenantId, Guid branchId, CancellationToken cancellationToken);
}
