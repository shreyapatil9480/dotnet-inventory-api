using FluentAssertions;
using InventoryApi.Application.Stock.Commands;
using InventoryApi.Domain.Entities;
using InventoryApi.Domain.Enums;
using InventoryApi.Domain.Exceptions;
using InventoryApi.Domain.Interfaces;
using Moq;

namespace InventoryApi.UnitTests.Stock;

public class UpdateStockHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IStockRepository> _stockRepo = new();

    [Fact]
    public async Task Handle_InboundMovement_AddsMovement()
    {
        var product = new Product("Widget", "WGT-001", 10m, 1);
        _productRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _stockRepo.Setup(r => r.GetCurrentStockLevelAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _stockRepo.Setup(r => r.AddMovementAsync(It.IsAny<StockMovement>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(7);

        var handler = new UpdateStockHandler(_productRepo.Object, _stockRepo.Object);
        var command = new UpdateStockCommand(1, MovementType.In, 5, Reference: "PO-001");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be(7);
        _stockRepo.Verify(r => r.AddMovementAsync(
            It.Is<StockMovement>(m => m.Type == MovementType.In && m.Quantity == 5),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OutboundExceedsStock_ThrowsInsufficientStockException()
    {
        var product = new Product("Widget", "WGT-001", 10m, 1);
        _productRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _stockRepo.Setup(r => r.GetCurrentStockLevelAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(3);

        var handler = new UpdateStockHandler(_productRepo.Object, _stockRepo.Object);
        var command = new UpdateStockCommand(1, MovementType.Out, 5);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InsufficientStockException>();
    }
}
