using AutoMapper;
using InventoryApi.Domain.Entities;
using InventoryApi.Domain.Interfaces;
using MediatR;

namespace InventoryApi.Application.Products.Commands;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;

    public CreateProductHandler(IProductRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<int> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        var product = _mapper.Map<Product>(cmd);
        return await _repo.AddAsync(product, ct);
    }
}
