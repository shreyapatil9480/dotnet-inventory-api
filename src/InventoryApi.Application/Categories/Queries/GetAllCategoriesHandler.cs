using AutoMapper;
using InventoryApi.Application.DTOs;
using InventoryApi.Domain.Interfaces;
using MediatR;

namespace InventoryApi.Application.Categories.Queries;

public class GetAllCategoriesHandler : IRequestHandler<GetAllCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly ICategoryRepository _repo;
    private readonly IMapper _mapper;

    public GetAllCategoriesHandler(ICategoryRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CategoryDto>> Handle(GetAllCategoriesQuery query, CancellationToken ct)
    {
        var categories = await _repo.GetAllAsync(ct);
        return _mapper.Map<IReadOnlyList<CategoryDto>>(categories);
    }
}
