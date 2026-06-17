using InventoryApi.Domain.Entities;
using InventoryApi.Domain.Enums;
using InventoryApi.Domain.Exceptions;

namespace InventoryApi.Application.Stock;

/// <summary>
/// Factory that encapsulates creation rules for each stock movement type.
/// </summary>
public static class StockMovementFactory
{
    public static StockMovement CreateInbound(int productId, int quantity, string supplierRef)
    {
        if (quantity <= 0)
            throw new DomainException("Inbound quantity must be positive.");
        if (string.IsNullOrWhiteSpace(supplierRef))
            throw new DomainException("Inbound movement requires a supplier reference.");

        return new StockMovement(productId, MovementType.In, quantity, supplierRef);
    }

    public static StockMovement CreateOutbound(int productId, int quantity, int currentStock)
    {
        if (quantity <= 0)
            throw new DomainException("Outbound quantity must be positive.");
        if (quantity > currentStock)
            throw new InsufficientStockException(productId, quantity, currentStock);

        return new StockMovement(productId, MovementType.Out, -quantity, "OUTBOUND");
    }

    public static StockMovement CreateAdjustment(int productId, int delta, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Adjustment requires a reason.");

        return new StockMovement(productId, MovementType.Adjustment, delta, reason);
    }
}
