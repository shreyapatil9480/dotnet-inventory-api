using InventoryApi.Domain.Enums;

namespace InventoryApi.Application.DTOs;

public record StockMovementDto(
    int Id,
    int ProductId,
    MovementType Type,
    int Quantity,
    string Reference,
    DateTime CreatedAt);
