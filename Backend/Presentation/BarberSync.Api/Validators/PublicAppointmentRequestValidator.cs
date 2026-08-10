using BarberSync.Api.Models.Public;
using FluentValidation;

namespace BarberSync.Api.Validators;

public sealed class PublicAppointmentRequestValidator : AbstractValidator<PublicAppointmentRequest>
{
    public PublicAppointmentRequestValidator()
    {
        RuleFor(x => x.ClientName).NotEmpty().WithMessage("Informe seu nome.").MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("Informe seu telefone.").Matches(@"^\+?[0-9 ()-]{10,20}$").WithMessage("Informe um telefone válido com DDD.");
        RuleFor(x => x.Email).EmailAddress().WithMessage("Informe um e-mail válido.").When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.ServiceId).NotEmpty().WithMessage("Escolha um serviço.");
        RuleFor(x => x.ScheduledAt).GreaterThan(DateTimeOffset.Now).WithMessage("Escolha uma data e hora futuras.");
        RuleFor(x => x.ScheduledAt).LessThan(DateTimeOffset.Now.AddMonths(6)).WithMessage("O agendamento deve ocorrer nos próximos seis meses.");
        RuleFor(x => x.Notes).MaximumLength(500).WithMessage("A observação deve ter no máximo 500 caracteres.");
    }
}
