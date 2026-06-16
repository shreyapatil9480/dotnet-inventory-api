using InventoryApi.Application.DTOs;
using MediatR;

namespace InventoryApi.Application.Categories.Queries;

public record GetAllCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;
