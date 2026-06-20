using FluentAssertions;
using InventoryApi.Application.Pricing;
using InventoryApi.Domain.Entities;

namespace InventoryApi.UnitTests.Pricing;

public class DiscountedPricingTests
{
    private readonly DiscountedPricing _strategy = new();
    private readonly Product _product = new("Widget", "WGT-001", 10m, 1);

    [Fact]
    public void CalculatePrice_BelowThreshold_NoDiscount()
    {
        var price = _strategy.CalculatePrice(_product, 9);

        price.Should().Be(90m);
    }

    [Fact]
    public void CalculatePrice_AtThreshold_AppliesDiscount()
    {
        var price = _strategy.CalculatePrice(_product, 10);

        price.Should().Be(90m);
    }

    [Fact]
    public void CalculatePrice_AboveThreshold_AppliesDiscount()
    {
        var price = _strategy.CalculatePrice(_product, 20);

        price.Should().Be(180m);
    }
}
