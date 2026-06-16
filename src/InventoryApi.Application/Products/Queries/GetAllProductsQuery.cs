using InventoryApi.Application.DTOs;
using MediatR;

namespace InventoryApi.Application.Products.Queries;

public record GetAllProductsQuery : IRequest<IReadOnlyList<ProductDto>>;
