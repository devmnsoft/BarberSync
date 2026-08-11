namespace BarberSync.Api.Services.Recognition;

public sealed record ServiceRecognitionEvent(Guid Id, Guid TenantId, Guid BranchId, Guid? CameraDeviceId, Guid? AppointmentId, DateTimeOffset OccurredAt, IReadOnlySet<string> Signals);
public sealed record ServiceRecognitionEvidence(Guid Id, Guid EventId, string EvidenceType, string? StorageUri, DateTimeOffset? ExpiresAt);
public sealed record ServiceRecognitionSuggestion(Guid Id, Guid EventId, string ServiceCategory, decimal Confidence, string Reason, string Status = "Pending");
public sealed record ServiceRecognitionConfirmation(Guid Id, Guid SuggestionId, string Decision, Guid? CorrectedServiceId, Guid? ServiceOrderId, Guid ConfirmedBy, DateTimeOffset ConfirmedAt);
public interface IServiceRecognitionProvider { Task<ServiceRecognitionSuggestion?> SuggestAsync(ServiceRecognitionEvent recognitionEvent, CancellationToken cancellationToken); }
public interface IDevRuleBasedRecognitionProvider : IServiceRecognitionProvider { }
public interface IServiceRecognitionService { Task<ServiceRecognitionSuggestion?> SuggestAsync(ServiceRecognitionEvent recognitionEvent, CancellationToken cancellationToken); }

public sealed class DevRuleBasedRecognitionProvider : IDevRuleBasedRecognitionProvider
{
    private static readonly (string Category, string[] Signals, decimal Confidence, string Reason)[] Rules =
    [
        ("Barba", ["chair-inclined","face-towel"], .82m, "Cadeira inclinada e toalha no rosto."),
        ("Corte", ["cape","scissors-or-clipper"], .85m, "Capa e tesoura ou máquina em uso."),
        ("Lavagem/Hidratação", ["washbasin","washing"], .88m, "Atividade registrada no lavatório."),
        ("Tratamento/Cauterização", ["product-application","pause-time"], .78m, "Aplicação de produto seguida de pausa."),
        ("Sobrancelha", ["eyebrow-zone","tweezers-or-razor"], .84m, "Zona de sobrancelha com pinça ou navalha."),
        ("Escova/Finalização", ["dryer-or-flatiron"], .80m, "Secador ou prancha detectado.")
    ];
    public Task<ServiceRecognitionSuggestion?> SuggestAsync(ServiceRecognitionEvent e, CancellationToken ct)
    {
        var rule = Rules.FirstOrDefault(r => r.Signals.All(e.Signals.Contains));
        ServiceRecognitionSuggestion? result = rule.Category is null ? null : new(Guid.NewGuid(),e.Id,rule.Category,rule.Confidence,rule.Reason);
        return Task.FromResult(result);
    }
}
public sealed class ServiceRecognitionService(IDevRuleBasedRecognitionProvider provider) : IServiceRecognitionService
{
    public Task<ServiceRecognitionSuggestion?> SuggestAsync(ServiceRecognitionEvent e, CancellationToken ct) => provider.SuggestAsync(e,ct);
}
