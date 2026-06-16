using AutoMapper;
using InventoryApi.Application.DTOs;
using InventoryApi.Domain.Interfaces;
using MediatR;

namespace InventoryApi.Application.Products.Queries;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;

    public GetProductByIdHandler(IProductRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery query, CancellationToken ct)
    {
        var product = await _repo.GetByIdAsync(query.Id, ct);
        return product is null ? null : _mapper.Map<ProductDto>(product);
    }
}
