using InventoryApi.Domain.Entities;
using InventoryApi.Domain.Interfaces;
using InventoryApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Infrastructure.Repositories;

public class SqliteCategoryRepository : ICategoryRepository
{
    private readonly InventoryDbContext _context;

    public SqliteCategoryRepository(InventoryDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Categories.FindAsync([id], ct);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<int> AddAsync(Category category, CancellationToken ct = default)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync(ct);
        return category.Id;
    }
}
