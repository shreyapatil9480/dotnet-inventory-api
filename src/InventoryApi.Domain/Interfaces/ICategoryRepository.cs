using InventoryApi.Domain.Entities;

namespace InventoryApi.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default);
    Task<int> AddAsync(Category category, CancellationToken ct = default);
}
