using AutoMapper;
using InventoryApi.Application.DTOs;
using InventoryApi.Application.Products.Commands;
using InventoryApi.Domain.Entities;

namespace InventoryApi.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name));

        CreateMap<CreateProductCommand, Product>()
            .ConstructUsing(cmd => new Product(cmd.Name, cmd.SKU, cmd.Price, cmd.CategoryId));

        CreateMap<Category, CategoryDto>();
        CreateMap<StockMovement, StockMovementDto>();
    }
}
