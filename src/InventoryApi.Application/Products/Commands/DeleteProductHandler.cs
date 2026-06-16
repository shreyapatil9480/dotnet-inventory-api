using InventoryApi.Domain.Exceptions;
using InventoryApi.Domain.Interfaces;
using MediatR;

namespace InventoryApi.Application.Products.Commands;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IProductRepository _repo;

    public DeleteProductHandler(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task Handle(DeleteProductCommand cmd, CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(cmd.Id, ct);
        if (product is null)
            throw new DomainException($"Product {cmd.Id} not found.");

        await _repo.DeleteAsync(cmd.Id, ct);
    }
}
