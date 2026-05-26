using BarberSync.Application.DTOs.Commercial;

namespace BarberSync.Application.Services.Commercial;

public class CrmLeadService
{
    private readonly List<CrmLeadDto> _leads = [];
    private readonly List<(Guid LeadId, DateTime ChangedAt, string Status, string? Reason)> _statusHistory = [];
    private readonly List<(Guid LeadId, DateTime At, string Channel, string Content)> _interactions = [];
    private readonly List<(Guid LeadId, DateTime At, string Content, Guid? Author)> _notes = [];

    public CrmLeadDto CreateLead(CreateCrmLeadRequest request)
    {
        if (_leads.Any(x => x.TenantId == request.TenantId && !x.IsLost && ((request.Phone is not null && x.Phone == request.Phone) || (request.Email is not null && x.Email == request.Email))))
            throw new InvalidOperationException("Já existe um lead com este telefone ou e-mail.");

        var lead = new CrmLeadDto(Guid.NewGuid(), request.TenantId, request.BranchId, request.FullName, request.Phone, request.Email, request.Source, "NEW", request.Temperature, request.AssignedToUserId, DateTime.UtcNow, null, false, false, null, null);
        _leads.Add(lead);
        _statusHistory.Add((lead.Id, DateTime.UtcNow, "NEW", "Criação"));
        return lead;
    }

    public IReadOnlyList<CrmLeadDto> GetLeads(Guid tenantId) => _leads.Where(x => x.TenantId == tenantId).ToList();
    public CrmLeadDto? GetById(Guid id) => _leads.FirstOrDefault(x => x.Id == id);

    public CrmLeadDto Assign(Guid id, Guid userId) => Update(id, x => x with { AssignedToUserId = userId });

    public CrmLeadDto UpdateStatus(Guid id, UpdateLeadStatusRequest request)
    {
        if (request.Status.Equals("LOST", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(request.Reason))
            throw new InvalidOperationException("Informe o motivo da perda.");
        var updated = Update(id, x => x with { Status = request.Status.ToUpperInvariant(), IsLost = request.Status.Equals("LOST", StringComparison.OrdinalIgnoreCase), LossReason = request.Reason });
        _statusHistory.Add((id, DateTime.UtcNow, updated.Status, request.Reason));
        return updated;
    }

    public CrmLeadDto Convert(Guid id, Guid clientId)
    {
        var lead = Require(id);
        if (lead.IsConverted) throw new InvalidOperationException("Lead já convertido.");
        var updated = lead with { IsConverted = true, Status = "CONVERTED", ConvertedClientId = clientId, IsLost = false };
        Replace(updated);
        _statusHistory.Add((id, DateTime.UtcNow, "CONVERTED", "Conversão"));
        return updated;
    }

    public CrmLeadDto Reopen(Guid id, string justification)
    {
        if (string.IsNullOrWhiteSpace(justification)) throw new InvalidOperationException("Informe justificativa para reabertura.");
        var updated = Update(id, x => x with { IsLost = false, Status = "REOPENED", LossReason = null });
        _statusHistory.Add((id, DateTime.UtcNow, "REOPENED", justification));
        return updated;
    }

    public void AddInteraction(Guid leadId, LeadInteractionRequest request)
    {
        var lead = Require(leadId);
        if (request.Channel.Equals("WHATSAPP", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(lead.Phone))
            throw new InvalidOperationException("Lead com telefone inválido não pode receber WhatsApp.");
        _interactions.Add((leadId, request.InteractedAt ?? DateTime.UtcNow, request.Channel, request.Content));
        Replace(lead with { LastContactAt = request.InteractedAt ?? DateTime.UtcNow });
    }

    public void AddNote(Guid leadId, LeadNoteRequest request)
    {
        Require(leadId);
        _notes.Add((leadId, DateTime.UtcNow, request.Content, request.AuthorUserId));
    }

    public IReadOnlyList<CrmLeadDto> HotLeads(Guid tenantId) => _leads.Where(x => x.TenantId == tenantId && x.Temperature.Equals("HOT", StringComparison.OrdinalIgnoreCase) && !x.IsLost && !x.IsConverted).ToList();
    public IReadOnlyList<CrmLeadDto> Duplicates(Guid tenantId) => _leads.Where(x => x.TenantId == tenantId).GroupBy(x => (x.Phone, x.Email)).Where(g => (!string.IsNullOrWhiteSpace(g.Key.Phone) || !string.IsNullOrWhiteSpace(g.Key.Email)) && g.Count() > 1).SelectMany(g => g).ToList();

    private CrmLeadDto Require(Guid id) => GetById(id) ?? throw new KeyNotFoundException("Lead não encontrado.");
    private CrmLeadDto Update(Guid id, Func<CrmLeadDto, CrmLeadDto> fn){var current=Require(id); var updated=fn(current); Replace(updated); return updated;}
    private void Replace(CrmLeadDto updated){var index=_leads.FindIndex(x=>x.Id==updated.Id); _leads[index]=updated;}
}
