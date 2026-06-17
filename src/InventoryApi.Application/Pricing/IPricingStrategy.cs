using InventoryApi.Domain.Entities;

namespace InventoryApi.Application.Pricing;

public interface IPricingStrategy
{
    decimal CalculatePrice(Product product, int quantity);
}
