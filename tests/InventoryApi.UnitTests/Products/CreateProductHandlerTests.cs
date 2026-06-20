using AutoMapper;
using FluentAssertions;
using InventoryApi.Application.Mapping;
using InventoryApi.Application.Products.Commands;
using InventoryApi.Domain.Entities;
using InventoryApi.Domain.Interfaces;
using Moq;

namespace InventoryApi.UnitTests.Products;

public class CreateProductHandlerTests
{
    private readonly Mock<IProductRepository> _mockRepo;
    private readonly IMapper _mapper;

    public CreateProductHandlerTests()
    {
        _mockRepo = new Mock<IProductRepository>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewProductId()
    {
        var handler = new CreateProductHandler(_mockRepo.Object, _mapper);
        var command = new CreateProductCommand("Widget A", "WGT-001", 9.99m, 1);

        _mockRepo.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) =>
            {
                return 42;
            });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(42);
        _mockRepo.Verify(r => r.AddAsync(
            It.Is<Product>(p => p.Name == "Widget A" && p.SKU == "WGT-001"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NegativePrice_ThrowsDomainException()
    {
        var validator = new CreateProductCommandValidator();
        var command = new CreateProductCommand("Widget", "WGT-001", -5.00m, 1);

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }
}
