using BarberSync.Application.DTOs;
using FluentValidation;

namespace BarberSync.Api.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.EffectiveUserIdentifier)
            .NotEmpty().WithMessage("Informe o CPF ou e-mail do usuário.")
            .MaximumLength(254).WithMessage("O identificador do usuário é inválido.");

        RuleFor(x => x.EffectiveTenantIdentifier)
            .MaximumLength(254).WithMessage("O identificador da empresa é inválido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .MinimumLength(8).WithMessage("A senha informada não atende aos requisitos de segurança.");
    }
}
