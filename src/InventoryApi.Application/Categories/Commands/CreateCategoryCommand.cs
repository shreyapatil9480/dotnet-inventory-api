using MediatR;

namespace InventoryApi.Application.Categories.Commands;

public record CreateCategoryCommand(string Name) : IRequest<int>;
