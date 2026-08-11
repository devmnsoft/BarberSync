using BarberSync.Application.DTOs;
using FluentValidation;

namespace BarberSync.Api.Validators;

public sealed class FirstAdminRequestValidator : AbstractValidator<FirstAdminRequestDto>
{
    public FirstAdminRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Informe o e-mail.").EmailAddress().WithMessage("Informe um e-mail válido.");
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Informe o nome completo.").MinimumLength(3).WithMessage("O nome deve ter ao menos 3 caracteres.");
        RuleFor(x => x.TenantSlug).Matches("^[a-z0-9][a-z0-9-]{2,59}$").WithMessage("O identificador da empresa deve ter de 3 a 60 caracteres minúsculos, números ou hífen.");
        RuleFor(x => x.BranchCode).Matches("^[A-Za-z0-9-]{2,30}$").WithMessage("O código da unidade deve ter de 2 a 30 letras, números ou hífen.");
        RuleFor(x => x.Password)
            .MinimumLength(12).WithMessage("A senha deve ter ao menos 12 caracteres.")
            .Matches("[A-Z]").WithMessage("A senha deve conter letra maiúscula.")
            .Matches("[a-z]").WithMessage("A senha deve conter letra minúscula.")
            .Matches("[0-9]").WithMessage("A senha deve conter número.")
            .Matches("[^A-Za-z0-9]").WithMessage("A senha deve conter caractere especial.");
    }
}

