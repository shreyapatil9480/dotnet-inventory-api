using AutoMapper;
using InventoryApi.Application.DTOs;
using InventoryApi.Domain.Interfaces;
using MediatR;

namespace InventoryApi.Application.Stock.Queries;

public class GetStockHistoryHandler : IRequestHandler<GetStockHistoryQuery, IReadOnlyList<StockMovementDto>?>
{
    private readonly IProductRepository _productRepo;
    private readonly IStockRepository _stockRepo;
    private readonly IMapper _mapper;

    public GetStockHistoryHandler(
        IProductRepository productRepo,
        IStockRepository stockRepo,
        IMapper mapper)
    {
        _productRepo = productRepo;
        _stockRepo = stockRepo;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<StockMovementDto>?> Handle(GetStockHistoryQuery query, CancellationToken ct)
    {
        var product = await _productRepo.GetByIdAsync(query.ProductId, ct);
        if (product is null)
            return null;

        var history = await _stockRepo.GetHistoryAsync(query.ProductId, ct);
        return _mapper.Map<IReadOnlyList<StockMovementDto>>(history);
    }
}
