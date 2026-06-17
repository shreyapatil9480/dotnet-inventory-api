using InventoryApi.Domain.Enums;
using InventoryApi.Domain.Exceptions;
using InventoryApi.Domain.Interfaces;
using MediatR;

namespace InventoryApi.Application.Stock.Commands;

public class UpdateStockHandler : IRequestHandler<UpdateStockCommand, int>
{
    private readonly IProductRepository _productRepo;
    private readonly IStockRepository _stockRepo;

    public UpdateStockHandler(IProductRepository productRepo, IStockRepository stockRepo)
    {
        _productRepo = productRepo;
        _stockRepo = stockRepo;
    }

    public async Task<int> Handle(UpdateStockCommand cmd, CancellationToken ct)
    {
        var product = await _productRepo.GetByIdAsync(cmd.ProductId, ct)
            ?? throw new DomainException($"Product {cmd.ProductId} not found.");

        var currentStock = await _stockRepo.GetCurrentStockLevelAsync(cmd.ProductId, ct);

        var movement = cmd.Type switch
        {
            MovementType.In => StockMovementFactory.CreateInbound(
                cmd.ProductId, cmd.Quantity, cmd.Reference!),
            MovementType.Out => StockMovementFactory.CreateOutbound(
                cmd.ProductId, cmd.Quantity, currentStock),
            MovementType.Adjustment => StockMovementFactory.CreateAdjustment(
                cmd.ProductId, cmd.Quantity, cmd.Reason!),
            _ => throw new DomainException($"Unsupported movement type: {cmd.Type}")
        };

        return await _stockRepo.AddMovementAsync(movement, ct);
    }
}
