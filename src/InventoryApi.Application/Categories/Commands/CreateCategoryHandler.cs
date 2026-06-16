using InventoryApi.Domain.Entities;
using InventoryApi.Domain.Interfaces;
using MediatR;

namespace InventoryApi.Application.Categories.Commands;

public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, int>
{
    private readonly ICategoryRepository _repo;

    public CreateCategoryHandler(ICategoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<int> Handle(CreateCategoryCommand cmd, CancellationToken ct)
    {
        var category = new Category(cmd.Name);
        return await _repo.AddAsync(category, ct);
    }
}
