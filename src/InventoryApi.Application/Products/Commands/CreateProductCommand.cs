using MediatR;

namespace InventoryApi.Application.Products.Commands;

public record CreateProductCommand(string Name, string SKU, decimal Price, int CategoryId)
    : IRequest<int>;
