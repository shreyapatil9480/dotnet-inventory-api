using AutoMapper;
using InventoryApi.Application.DTOs;
using InventoryApi.Domain.Interfaces;
using MediatR;

namespace InventoryApi.Application.Products.Queries;

public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductRepository _repo;
    private readonly IMapper _mapper;

    public GetAllProductsHandler(IProductRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ProductDto>> Handle(GetAllProductsQuery query, CancellationToken ct)
    {
        var products = await _repo.GetAllAsync(ct);
        return _mapper.Map<IReadOnlyList<ProductDto>>(products);
    }
}
