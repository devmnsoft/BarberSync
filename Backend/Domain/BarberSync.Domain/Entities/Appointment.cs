using BarberSync.Domain.Common;
using BarberSync.Domain.Enums;
using System;

namespace BarberSync.Domain.Entities;

public sealed class Appointment : BaseEntity
{
    public Guid ClientId { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public Guid ServiceId { get; private set; }
    public DateTimeOffset ScheduledStart { get; private set; }
    public DateTimeOffset ScheduledEnd { get; private set; }
    public string Origin { get; private set; } = "Admin";
    public new AppointmentStatus Status { get; private set; } = AppointmentStatus.Scheduled;
    public string? Notes { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTimeOffset? CheckedInAt { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private Appointment() { }

    public Appointment(Guid tenantId, Guid branchId, Guid clientId, Guid professionalId, Guid serviceId,
        DateTimeOffset scheduledStart, DateTimeOffset scheduledEnd, string origin = "Admin", string? notes = null)
    {
        if (tenantId == Guid.Empty || branchId == Guid.Empty || clientId == Guid.Empty || professionalId == Guid.Empty || serviceId == Guid.Empty)
            throw new ArgumentException("Tenant, unidade, cliente, profissional e serviço são obrigatórios.");
        EnsurePeriod(scheduledStart, scheduledEnd);
        TenantId = tenantId; BranchId = branchId; ClientId = clientId; ProfessionalId = professionalId;
        ServiceId = serviceId; ScheduledStart = scheduledStart; ScheduledEnd = scheduledEnd;
        Origin = string.IsNullOrWhiteSpace(origin) ? "Admin" : origin.Trim(); Notes = notes?.Trim();
    }

    public void Confirm() { Require(AppointmentStatus.Scheduled); Status = AppointmentStatus.Confirmed; }
    public void CheckIn(DateTimeOffset? at = null) { Require(AppointmentStatus.Confirmed); Status = AppointmentStatus.CheckedIn; CheckedInAt = at ?? DateTimeOffset.UtcNow; }
    public void Start(DateTimeOffset? at = null) { Require(AppointmentStatus.CheckedIn); Status = AppointmentStatus.InService; StartedAt = at ?? DateTimeOffset.UtcNow; }
    public void Finish(DateTimeOffset? at = null) { Require(AppointmentStatus.InService); Status = AppointmentStatus.AwaitingPayment; CompletedAt = at ?? DateTimeOffset.UtcNow; }
    public void MarkPaid() { Require(AppointmentStatus.AwaitingPayment); Status = AppointmentStatus.Finished; }
    public void Cancel(string reason)
    {
        if (Status is AppointmentStatus.Finished or AppointmentStatus.Cancelled or AppointmentStatus.NoShow) throw InvalidTransition(AppointmentStatus.Cancelled);
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("O motivo do cancelamento é obrigatório.", nameof(reason));
        CancellationReason = reason.Trim(); Status = AppointmentStatus.Cancelled;
    }
    public void MarkNoShow()
    {
        if (Status is not (AppointmentStatus.Scheduled or AppointmentStatus.Confirmed)) throw InvalidTransition(AppointmentStatus.NoShow);
        Status = AppointmentStatus.NoShow;
    }
    public void Reschedule(DateTimeOffset start, DateTimeOffset end)
    {
        if (Status is not (AppointmentStatus.Scheduled or AppointmentStatus.Confirmed)) throw new InvalidOperationException("Somente agendamentos pendentes podem ser reagendados.");
        EnsurePeriod(start, end); ScheduledStart = start; ScheduledEnd = end; Status = AppointmentStatus.Scheduled;
    }
    private void Require(AppointmentStatus expected) { if (Status != expected) throw InvalidTransition(expected); }
    private InvalidOperationException InvalidTransition(AppointmentStatus target) => new($"Transição de {Status} para {target} não permitida.");
    private static void EnsurePeriod(DateTimeOffset start, DateTimeOffset end) { if (end <= start) throw new ArgumentException("O término deve ocorrer após o início."); }
}
