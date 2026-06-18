using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InventoryApi.Infrastructure.Data;

/// <summary>
/// Design-time factory for EF Core migrations without requiring the API startup project.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
    public InventoryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
        optionsBuilder.UseSqlite("Data Source=inventory.db");
        return new InventoryDbContext(optionsBuilder.Options);
    }
}
