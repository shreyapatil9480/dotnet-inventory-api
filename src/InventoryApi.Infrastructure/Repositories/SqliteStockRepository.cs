using InventoryApi.Domain.Entities;
using InventoryApi.Domain.Interfaces;
using InventoryApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Infrastructure.Repositories;

public class SqliteStockRepository : IStockRepository
{
    private readonly InventoryDbContext _context;

    public SqliteStockRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetCurrentStockLevelAsync(int productId, CancellationToken ct = default)
    {
        var movements = await _context.StockMovements
            .Where(m => m.ProductId == productId)
            .ToListAsync(ct);

        return movements.Sum(m => m.Quantity);
    }

    public async Task<int> AddMovementAsync(StockMovement movement, CancellationToken ct = default)
    {
        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync(ct);
        return movement.Id;
    }

    public async Task<IReadOnlyList<StockMovement>> GetHistoryAsync(int productId, CancellationToken ct = default)
    {
        return await _context.StockMovements
            .Where(m => m.ProductId == productId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);
    }
}
