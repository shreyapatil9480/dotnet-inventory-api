using InventoryApi.Domain.Entities;

namespace InventoryApi.Application.Pricing;

public class StandardPricing : IPricingStrategy
{
    public decimal CalculatePrice(Product product, int quantity) =>
        product.Price * quantity;
}
