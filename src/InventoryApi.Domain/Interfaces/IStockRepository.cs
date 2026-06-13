using InventoryApi.Domain.Entities;

namespace InventoryApi.Domain.Interfaces;

public interface IStockRepository
{
    Task<int> GetCurrentStockLevelAsync(int productId, CancellationToken ct = default);
    Task<int> AddMovementAsync(StockMovement movement, CancellationToken ct = default);
    Task<IReadOnlyList<StockMovement>> GetHistoryAsync(int productId, CancellationToken ct = default);
}
