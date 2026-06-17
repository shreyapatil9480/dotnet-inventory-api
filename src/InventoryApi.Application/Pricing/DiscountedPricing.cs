using InventoryApi.Domain.Entities;

namespace InventoryApi.Application.Pricing;

public class DiscountedPricing : IPricingStrategy
{
    private const int BulkThreshold = 10;
    private const decimal DiscountRate = 0.90m;

    public decimal CalculatePrice(Product product, int quantity)
    {
        var basePrice = product.Price * quantity;
        return quantity >= BulkThreshold ? basePrice * DiscountRate : basePrice;
    }
}
