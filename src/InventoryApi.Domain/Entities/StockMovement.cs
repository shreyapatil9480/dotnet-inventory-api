using InventoryApi.Domain.Enums;

namespace InventoryApi.Domain.Entities;

public class StockMovement
{
    public int Id { get; private set; }
    public int ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public MovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private StockMovement()
    {
    }

    public StockMovement(int productId, MovementType type, int quantity, string reference)
    {
        ProductId = productId;
        Type = type;
        Quantity = quantity;
        Reference = reference;
        CreatedAt = DateTime.UtcNow;
    }
}
