using MediatR;

namespace InventoryApi.Application.Stock.Queries;

public record GetStockLevelQuery(int ProductId) : IRequest<int?>;
