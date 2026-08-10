using BarberSync.Application.Operations;
using FluentValidation;

namespace BarberSync.Api.Validators;

public sealed class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentRequestValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty(); RuleFor(x => x.ProfessionalId).NotEmpty(); RuleFor(x => x.ServiceId).NotEmpty();
        RuleFor(x => x.ScheduledStart).NotEmpty(); RuleFor(x => x.Origin).NotEmpty().MaximumLength(30); RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public sealed class UpdateAppointmentRequestValidator : AbstractValidator<UpdateAppointmentRequest>
{
    public UpdateAppointmentRequestValidator()
    {
        Include(new CreateAppointmentRequestValidatorAdapter());
    }
    private sealed class CreateAppointmentRequestValidatorAdapter : AbstractValidator<UpdateAppointmentRequest>
    {
        public CreateAppointmentRequestValidatorAdapter()
        { RuleFor(x=>x.ClientId).NotEmpty();RuleFor(x=>x.ProfessionalId).NotEmpty();RuleFor(x=>x.ServiceId).NotEmpty();RuleFor(x=>x.ScheduledStart).NotEmpty();RuleFor(x=>x.Origin).NotEmpty().MaximumLength(30);RuleFor(x=>x.Notes).MaximumLength(2000); }
    }
}

public sealed class CancelAppointmentRequestValidator : AbstractValidator<CancelAppointmentRequest>
{ public CancelAppointmentRequestValidator() => RuleFor(x=>x.Reason).NotEmpty().MinimumLength(3).MaximumLength(500); }

public sealed class RescheduleAppointmentRequestValidator : AbstractValidator<RescheduleAppointmentRequest>
{ public RescheduleAppointmentRequestValidator() { RuleFor(x=>x.ScheduledStart).NotEmpty();RuleFor(x=>x.Reason).NotEmpty().MinimumLength(3).MaximumLength(500); } }
