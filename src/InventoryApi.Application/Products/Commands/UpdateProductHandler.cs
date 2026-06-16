using InventoryApi.Domain.Exceptions;
using InventoryApi.Domain.Interfaces;
using MediatR;

namespace InventoryApi.Application.Products.Commands;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IProductRepository _repo;

    public UpdateProductHandler(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task Handle(UpdateProductCommand cmd, CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new DomainException($"Product {cmd.Id} not found.");

        product.UpdateDetails(cmd.Name, cmd.SKU, cmd.Price, cmd.CategoryId);
        await _repo.UpdateAsync(product, ct);
    }
}
