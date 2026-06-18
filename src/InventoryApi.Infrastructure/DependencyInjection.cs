using InventoryApi.Domain.Interfaces;
using InventoryApi.Infrastructure.Data;
using InventoryApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IProductRepository, SqliteProductRepository>();
        services.AddScoped<IStockRepository, SqliteStockRepository>();
        services.AddScoped<ICategoryRepository, SqliteCategoryRepository>();

        return services;
    }
}
