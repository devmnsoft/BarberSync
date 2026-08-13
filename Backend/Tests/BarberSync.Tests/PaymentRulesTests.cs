using BarberSync.Application.Operations;
using FluentValidation;
using System;

namespace BarberSync.Tests;

public sealed class PaymentRulesTests
{
    [Fact]
    public void Partial_payment_keeps_order_partially_paid()
    {
        var amount = PaymentRules.ValidateAndTotal([new("Pix", 40)], 100);

        Assert.Equal(40, amount);
        Assert.Equal("PartiallyPaid", PaymentRules.OrderStatus(amount, 100));
    }

    [Fact]
    public void Mixed_payment_for_entire_balance_marks_order_paid()
    {
        var amount = PaymentRules.ValidateAndTotal([new("Card", 60), new("Cash", 40)], 100);

        Assert.Equal("Paid", PaymentRules.OrderStatus(amount, 100));
    }

    [Fact]
    public void Payment_cannot_exceed_balance()
    {
        var error = Assert.Throws<InvalidOperationException>(() =>
            PaymentRules.ValidateAndTotal([new("Pix", 100.01m)], 100));

        Assert.Contains("superar", error.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Split_values_must_be_positive(decimal amount)
    {
        Assert.Throws<InvalidOperationException>(() =>
            PaymentRules.ValidateAndTotal([new("Cash", amount)], 100));
    }

    [Fact]
    public void Cash_can_report_received_amount_and_change()
    {
        var amount = PaymentRules.ValidateAndTotal([new("Cash", 100, 120)], 100);
        Assert.Equal(100, amount);
    }

    [Fact]
    public void Non_cash_payment_cannot_generate_change()
    {
        Assert.Throws<InvalidOperationException>(() =>
            PaymentRules.ValidateAndTotal([new("Pix", 100, 120)], 100));
    }
    [Theory]
    [InlineData("Cash")]
    [InlineData("Pix")]
    [InlineData("DebitCard")]
    [InlineData("CreditCard")]
    public void Api_validator_accepts_domain_payment_methods(string method)
    {
        var validator = new BarberSync.Api.Validators.RegisterPaymentRequestValidator();
        var result = validator.Validate(new RegisterPaymentRequest("checkout-1", [new(method, 10)]));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Api_validator_rejects_legacy_payment_method()
    {
        var validator = new BarberSync.Api.Validators.RegisterPaymentRequestValidator();
        var result = validator.Validate(new RegisterPaymentRequest("checkout-1", [new("Debit", 10)]));

        Assert.Contains(result.Errors, error => error.PropertyName.Contains("Method"));
    }

}
