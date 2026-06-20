using FluentAssertions;
using InventoryApi.Application.Products.Commands;
using InventoryApi.Domain.Entities;
using InventoryApi.Domain.Exceptions;
using InventoryApi.Domain.Interfaces;
using Moq;

namespace InventoryApi.UnitTests.Products;

public class UpdateProductHandlerTests
{
    private readonly Mock<IProductRepository> _mockRepo = new();

    [Fact]
    public async Task Handle_ExistingProduct_UpdatesAndSaves()
    {
        var product = new Product("Old Name", "OLD-001", 10m, 1);
        _mockRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var handler = new UpdateProductHandler(_mockRepo.Object);
        var command = new UpdateProductCommand(1, "New Name", "NEW-001", 15m, 2);

        await handler.Handle(command, CancellationToken.None);

        product.Name.Should().Be("New Name");
        product.SKU.Should().Be("NEW-001");
        product.Price.Should().Be(15m);
        _mockRepo.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsDomainException()
    {
        _mockRepo.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new UpdateProductHandler(_mockRepo.Object);
        var command = new UpdateProductCommand(99, "Name", "SKU-001", 10m, 1);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}
