using MediatR;

namespace InventoryApi.Application.Products.Commands;

public record UpdateProductCommand(int Id, string Name, string SKU, decimal Price, int CategoryId)
    : IRequest;
