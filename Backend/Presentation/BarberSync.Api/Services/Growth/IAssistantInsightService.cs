namespace BarberSync.Api.Services.Growth;

public interface IAssistantInsightService
{
    Task<IReadOnlyList<AssistantInsight>> GetDashboardAsync(Guid tenantId, Guid branchId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AssistantInsight>> GetClientAsync(Guid tenantId, Guid clientId, CancellationToken cancellationToken);
}

public sealed record AssistantInsight(string Type, string Priority, string Message, string? ActionUrl = null);
