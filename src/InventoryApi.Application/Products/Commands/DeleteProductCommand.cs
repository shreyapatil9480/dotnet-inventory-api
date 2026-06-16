using MediatR;

namespace InventoryApi.Application.Products.Commands;

public record DeleteProductCommand(int Id) : IRequest;
