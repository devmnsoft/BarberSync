using BarberSync.Api.Validators;
using BarberSync.Application.Operations;
using FluentValidation;

namespace BarberSync.Tests;

public sealed class CashRegisterValidatorTests
{
    [Fact]
    public void Movement_requires_positive_amount_and_reason()
    {
        var result = new CashMovementRequestValidator().Validate(new CashMovementRequest(0, ""));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CashMovementRequest.Amount));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CashMovementRequest.Reason));
    }

    [Fact]
    public void Opening_and_closing_reject_negative_balances()
    {
        var opening = new OpenCashRegisterRequestValidator().Validate(new OpenCashRegisterRequest(-1));
        var closing = new CloseCashRegisterRequestValidator().Validate(new CloseCashRegisterRequest(-1));

        Assert.Contains(opening.Errors, error => error.PropertyName == nameof(OpenCashRegisterRequest.OpeningBalance));
        Assert.Contains(closing.Errors, error => error.PropertyName == nameof(CloseCashRegisterRequest.ActualBalance));
    }
}
