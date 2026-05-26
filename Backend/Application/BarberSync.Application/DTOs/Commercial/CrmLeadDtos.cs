namespace BarberSync.Application.DTOs.Commercial;

public record CrmLeadDto(Guid Id, Guid TenantId, Guid? BranchId, string FullName, string? Phone, string? Email, string Source, string Status, string Temperature, Guid? AssignedToUserId, DateTime CreatedAt, DateTime? LastContactAt, bool IsConverted, bool IsLost, string? LossReason, Guid? ConvertedClientId);
public record CreateCrmLeadRequest(Guid TenantId, Guid? BranchId, string FullName, string? Phone, string? Email, string Source, string Temperature, Guid? AssignedToUserId);
public record UpdateLeadStatusRequest(string Status, string? Reason);
public record AssignLeadRequest(Guid AssignedToUserId);
public record LeadNoteRequest(string Content, Guid? AuthorUserId);
public record LeadInteractionRequest(string Channel, string Content, DateTime? InteractedAt);
public record ConvertLeadRequest(Guid ClientId);
public record MarkLostRequest(string Reason);
public record ReopenLeadRequest(string Justification);
