using InventoryApi.Application.DTOs;
using MediatR;

namespace InventoryApi.Application.Stock.Queries;

public record GetStockHistoryQuery(int ProductId) : IRequest<IReadOnlyList<StockMovementDto>?>;
