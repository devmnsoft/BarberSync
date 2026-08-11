using BarberSync.Application.Operations;

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
}
