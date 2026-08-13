using BarberSync.Application.Operations;
using System;

namespace BarberSync.Tests;

public sealed class CashRegisterRulesTests
{
    [Fact]
    public void Opening_rejects_negative_balance() =>
        Assert.Throws<InvalidOperationException>(() => CashRegisterRules.ValidateOpening(-0.01m));

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Movement_rejects_non_positive_amount(decimal amount) =>
        Assert.Throws<InvalidOperationException>(() => CashRegisterRules.ValidateMovement(new(amount, "Motivo")));

    [Fact]
    public void Withdrawal_requires_reason() =>
        Assert.Throws<InvalidOperationException>(() => CashRegisterRules.ValidateMovement(new(10, " ")));

    [Fact]
    public void Expense_requires_category() =>
        Assert.Throws<InvalidOperationException>(() => CashRegisterRules.ValidateMovement(new(10, "Material"), true));

    [Fact]
    public void Closing_with_difference_requires_note() =>
        Assert.Throws<InvalidOperationException>(() => CashRegisterRules.ValidateClosing(100, new(99, null)));

    [Fact]
    public void Closing_without_difference_accepts_empty_note() =>
        CashRegisterRules.ValidateClosing(100, new(100, null));
}
