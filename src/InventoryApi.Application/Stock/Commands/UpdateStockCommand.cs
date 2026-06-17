using InventoryApi.Domain.Enums;
using MediatR;

namespace InventoryApi.Application.Stock.Commands;

public record UpdateStockCommand(
    int ProductId,
    MovementType Type,
    int Quantity,
    string? Reference = null,
    string? Reason = null) : IRequest<int>;
