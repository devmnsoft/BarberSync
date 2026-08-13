using BarberSync.Application.Operations;
using FluentValidation;

namespace BarberSync.Api.Validators;

public sealed class OpenCashRegisterRequestValidator : AbstractValidator<OpenCashRegisterRequest>
{
    public OpenCashRegisterRequestValidator()
    {
        RuleFor(x => x.OpeningBalance).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}

public sealed class CashMovementRequestValidator : AbstractValidator<CashMovementRequest>
{
    public CashMovementRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Category).MaximumLength(100);
    }
}

public sealed class CloseCashRegisterRequestValidator : AbstractValidator<CloseCashRegisterRequest>
{
    public CloseCashRegisterRequestValidator()
    {
        RuleFor(x => x.ActualBalance).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
