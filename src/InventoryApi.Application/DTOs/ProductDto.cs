namespace InventoryApi.Application.DTOs;

public record ProductDto(
    int Id,
    string Name,
    string SKU,
    decimal Price,
    int CategoryId,
    string CategoryName);
