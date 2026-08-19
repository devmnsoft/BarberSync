namespace BarberSync.Application.Operations;

public sealed record CreateAppointmentRequest(Guid ClientId, Guid ProfessionalId, Guid ServiceId, DateTimeOffset ScheduledStart, string Origin = "Admin", string? Notes = null);
public sealed record UpdateAppointmentRequest(Guid ClientId, Guid ProfessionalId, Guid ServiceId, DateTimeOffset ScheduledStart, string Origin, string? Notes);
public sealed record RescheduleAppointmentRequest(DateTimeOffset ScheduledStart, string Reason);
public sealed record CancelAppointmentRequest(string Reason);
public sealed record NoShowAppointmentRequest(string Reason);
public sealed record AppointmentFilter(DateTimeOffset? From, DateTimeOffset? To, Guid? ProfessionalId, Guid? ServiceId, string? Status, string? Origin);
public sealed record AvailabilityRequest(Guid ProfessionalId, Guid ServiceId, DateTimeOffset Start);
public sealed record AppointmentResponse(Guid Id, Guid ClientId, string ClientName, Guid ProfessionalId, string ProfessionalName, Guid ServiceId, string ServiceName, DateTimeOffset ScheduledStart, DateTimeOffset ScheduledEnd, int DurationMinutes, string Status, string Origin, string? Notes, string? CancellationReason);
public sealed record AppointmentDraft(Guid Id, Guid TenantId, Guid BranchId, Guid ClientId, Guid ProfessionalId, Guid ServiceId, DateTimeOffset Start, DateTimeOffset End, string Status, string Origin, string? Notes);

public interface IAppointmentRepository
{
    Task<IReadOnlyList<AppointmentResponse>> ListAsync(Guid tenantId, Guid branchId, AppointmentFilter filter, CancellationToken ct);
    Task<AppointmentResponse?> GetAsync(Guid tenantId, Guid branchId, Guid id, CancellationToken ct);
    Task<int?> GetServiceDurationAsync(Guid tenantId, Guid branchId, Guid serviceId, CancellationToken ct);
    Task<bool> HasConflictAsync(Guid tenantId, Guid branchId, Guid professionalId, DateTimeOffset start, DateTimeOffset end, Guid? exceptId, CancellationToken ct);
    Task<string?> GetUnavailabilityReasonAsync(Guid tenantId, Guid branchId, Guid professionalId, Guid serviceId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct);
    Task<AppointmentResponse> CreateAsync(AppointmentDraft appointment, CancellationToken ct);
    Task<AppointmentResponse> UpdateAsync(AppointmentDraft appointment, CancellationToken ct);
    Task<AppointmentResponse> ChangeStatusAsync(Guid tenantId, Guid branchId, Guid id, string expectedStatus, string status, string? reason, Guid userId, CancellationToken ct);
    Task<AppointmentResponse> RescheduleAsync(Guid tenantId, Guid branchId, Guid id, DateTimeOffset start, DateTimeOffset end, string reason, Guid userId, CancellationToken ct);
}

public interface IAppointmentService
{
    Task<IReadOnlyList<AppointmentResponse>> ListAsync(AppointmentFilter filter, CancellationToken ct);
    Task<AppointmentResponse?> GetAsync(Guid id, CancellationToken ct);
    Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken ct);
    Task<AppointmentResponse> UpdateAsync(Guid id, UpdateAppointmentRequest request, CancellationToken ct);
    Task<AppointmentResponse> ChangeStatusAsync(Guid id, string status, string? reason, CancellationToken ct);
    Task<AppointmentResponse> RescheduleAsync(Guid id, RescheduleAppointmentRequest request, CancellationToken ct);
    Task<bool> IsAvailableAsync(AvailabilityRequest request, CancellationToken ct);
    Task<IReadOnlyList<DateTimeOffset>> SmartSlotsAsync(Guid professionalId, Guid serviceId, DateOnly date, CancellationToken ct);
}

public sealed class AppointmentService(IAppointmentRepository repository, Abstractions.ICurrentUserContext currentUser) : IAppointmentService
{
    private static readonly IReadOnlyDictionary<string, string[]> Transitions = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        ["Confirmed"] = ["Scheduled"], ["CheckedIn"] = ["Scheduled", "Confirmed"], ["InService"] = ["CheckedIn"],
        ["Finished"] = ["InService"], ["Cancelled"] = ["Scheduled", "Confirmed", "CheckedIn"], ["NoShow"] = ["Scheduled", "Confirmed"]
    };

    public Task<IReadOnlyList<AppointmentResponse>> ListAsync(AppointmentFilter filter, CancellationToken ct) => repository.ListAsync(currentUser.TenantId, currentUser.BranchId, filter, ct);
    public Task<AppointmentResponse?> GetAsync(Guid id, CancellationToken ct) => repository.GetAsync(currentUser.TenantId, currentUser.BranchId, id, ct);

    public async Task<AppointmentResponse> CreateAsync(CreateAppointmentRequest request, CancellationToken ct)
    {
        var duration = await Duration(request.ServiceId, ct);
        var end = request.ScheduledStart.AddMinutes(duration);
        await EnsureAvailable(request.ProfessionalId, request.ServiceId, request.ScheduledStart, end, null, ct);
        return await repository.CreateAsync(new(Guid.NewGuid(), currentUser.TenantId, currentUser.BranchId, request.ClientId, request.ProfessionalId, request.ServiceId, request.ScheduledStart, end, "Scheduled", request.Origin, request.Notes), ct);
    }

    public async Task<AppointmentResponse> UpdateAsync(Guid id, UpdateAppointmentRequest request, CancellationToken ct)
    {
        _ = await Required(id, ct);
        var end = request.ScheduledStart.AddMinutes(await Duration(request.ServiceId, ct));
        await EnsureAvailable(request.ProfessionalId, request.ServiceId, request.ScheduledStart, end, id, ct);
        return await repository.UpdateAsync(new(id, currentUser.TenantId, currentUser.BranchId, request.ClientId, request.ProfessionalId, request.ServiceId, request.ScheduledStart, end, "Scheduled", request.Origin, request.Notes), ct);
    }

    public async Task<AppointmentResponse> ChangeStatusAsync(Guid id, string status, string? reason, CancellationToken ct)
    {
        if (!Transitions.TryGetValue(status, out var allowed)) throw new InvalidOperationException("Status de destino inválido.");
        var current = await Required(id, ct);
        if (!allowed.Contains(current.Status, StringComparer.OrdinalIgnoreCase)) throw new InvalidOperationException($"Transição de {current.Status} para {status} não permitida.");
        if (status is "Cancelled" or "NoShow" && string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException(status == "Cancelled" ? "O motivo do cancelamento é obrigatório." : "O motivo da ausência é obrigatório.");
        return await repository.ChangeStatusAsync(currentUser.TenantId, currentUser.BranchId, id, current.Status, status, reason?.Trim(), currentUser.UserId, ct);
    }

    public async Task<AppointmentResponse> RescheduleAsync(Guid id, RescheduleAppointmentRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Reason)) throw new InvalidOperationException("O motivo do reagendamento é obrigatório.");
        var current = await Required(id, ct);
        if (current.Status is "Cancelled" or "NoShow" or "Finished") throw new InvalidOperationException("Este agendamento não pode ser reagendado.");
        var end = request.ScheduledStart.AddMinutes(current.DurationMinutes);
        await EnsureAvailable(current.ProfessionalId, current.ServiceId, request.ScheduledStart, end, id, ct);
        return await repository.RescheduleAsync(currentUser.TenantId, currentUser.BranchId, id, request.ScheduledStart, end, request.Reason.Trim(), currentUser.UserId, ct);
    }

    public async Task<bool> IsAvailableAsync(AvailabilityRequest request, CancellationToken ct)
    {
        var end = request.Start.AddMinutes(await Duration(request.ServiceId, ct));
        return await repository.GetUnavailabilityReasonAsync(currentUser.TenantId, currentUser.BranchId, request.ProfessionalId, request.ServiceId, request.Start, end, ct) is null
            && !await repository.HasConflictAsync(currentUser.TenantId, currentUser.BranchId, request.ProfessionalId, request.Start, end, null, ct);
    }

    public async Task<IReadOnlyList<DateTimeOffset>> SmartSlotsAsync(Guid professionalId, Guid serviceId, DateOnly date, CancellationToken ct)
    {
        var duration = await Duration(serviceId, ct);
        var offset = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo").GetUtcOffset(date.ToDateTime(new TimeOnly(9, 0)));
        var cursor = new DateTimeOffset(date.ToDateTime(new TimeOnly(9, 0)), offset);
        var endOfDay = new DateTimeOffset(date.ToDateTime(new TimeOnly(19, 0)), offset);
        var result = new List<DateTimeOffset>();
        while (cursor.AddMinutes(duration) <= endOfDay)
        {
            if (await repository.GetUnavailabilityReasonAsync(currentUser.TenantId, currentUser.BranchId, professionalId, serviceId, cursor, cursor.AddMinutes(duration), ct) is null
                && !await repository.HasConflictAsync(currentUser.TenantId, currentUser.BranchId, professionalId, cursor, cursor.AddMinutes(duration), null, ct)) result.Add(cursor);
            cursor = cursor.AddMinutes(15);
        }
        return result;
    }

    private async Task<int> Duration(Guid serviceId, CancellationToken ct) => await repository.GetServiceDurationAsync(currentUser.TenantId, currentUser.BranchId, serviceId, ct) ?? throw new KeyNotFoundException("Serviço não encontrado.");
    private async Task<AppointmentResponse> Required(Guid id, CancellationToken ct) => await GetAsync(id, ct) ?? throw new KeyNotFoundException("Agendamento não encontrado.");
    private async Task EnsureAvailable(Guid professional, Guid service, DateTimeOffset start, DateTimeOffset end, Guid? except, CancellationToken ct)
    {
        if (start >= end) throw new InvalidOperationException("O término deve ocorrer após o início.");
        var reason = await repository.GetUnavailabilityReasonAsync(currentUser.TenantId, currentUser.BranchId, professional, service, start, end, ct);
        if (reason is not null) throw new InvalidOperationException(reason);
        if (await repository.HasConflictAsync(currentUser.TenantId, currentUser.BranchId, professional, start, end, except, ct)) throw new InvalidOperationException("O profissional já possui compromisso neste horário.");
    }
}
