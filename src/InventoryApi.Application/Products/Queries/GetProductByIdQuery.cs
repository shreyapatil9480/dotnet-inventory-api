using InventoryApi.Application.DTOs;
using MediatR;

namespace InventoryApi.Application.Products.Queries;

public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;
