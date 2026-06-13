using InventoryApi.Domain.Exceptions;

namespace InventoryApi.Domain.Entities;

public class Product
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string SKU { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public ICollection<StockMovement> StockMovements { get; private set; } = new List<StockMovement>();

    private Product()
    {
    }

    public Product(string name, string sku, decimal price, int categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required.");
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("Product SKU is required.");
        if (price <= 0)
            throw new DomainException("Price must be greater than zero.");
        if (categoryId <= 0)
            throw new DomainException("CategoryId must be greater than zero.");

        Name = name;
        SKU = sku;
        Price = price;
        CategoryId = categoryId;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new DomainException("Price must be greater than zero.");

        Price = newPrice;
    }

    public void UpdateDetails(string name, string sku, decimal price, int categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Product name is required.");
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("Product SKU is required.");
        if (price <= 0)
            throw new DomainException("Price must be greater than zero.");
        if (categoryId <= 0)
            throw new DomainException("CategoryId must be greater than zero.");

        Name = name;
        SKU = sku;
        Price = price;
        CategoryId = categoryId;
    }
}
