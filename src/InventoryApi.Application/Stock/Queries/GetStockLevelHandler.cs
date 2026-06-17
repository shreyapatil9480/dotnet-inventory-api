using InventoryApi.Domain.Interfaces;
using MediatR;

namespace InventoryApi.Application.Stock.Queries;

public class GetStockLevelHandler : IRequestHandler<GetStockLevelQuery, int?>
{
    private readonly IProductRepository _productRepo;
    private readonly IStockRepository _stockRepo;

    public GetStockLevelHandler(IProductRepository productRepo, IStockRepository stockRepo)
    {
        _productRepo = productRepo;
        _stockRepo = stockRepo;
    }

    public async Task<int?> Handle(GetStockLevelQuery query, CancellationToken ct)
    {
        var product = await _productRepo.GetByIdAsync(query.ProductId, ct);
        if (product is null)
            return null;

        return await _stockRepo.GetCurrentStockLevelAsync(query.ProductId, ct);
    }
}
