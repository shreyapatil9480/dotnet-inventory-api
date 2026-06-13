namespace InventoryApi.Domain.Exceptions;

/// <summary>
/// Thrown when an outbound stock movement would reduce inventory below zero.
/// </summary>
public class InsufficientStockException : DomainException
{
    public int ProductId { get; }
    public int RequestedQuantity { get; }
    public int CurrentStock { get; }

    public InsufficientStockException(int productId, int requestedQuantity, int currentStock)
        : base($"Insufficient stock for product {productId}. Requested: {requestedQuantity}, Available: {currentStock}.")
    {
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
        CurrentStock = currentStock;
    }
}
