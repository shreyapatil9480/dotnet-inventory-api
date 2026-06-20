using FluentAssertions;
using InventoryApi.Application.Stock;
using InventoryApi.Domain.Enums;
using InventoryApi.Domain.Exceptions;

namespace InventoryApi.UnitTests.Stock;

public class StockMovementFactoryTests
{
    [Fact]
    public void CreateInbound_ValidInputs_ReturnsInboundMovement()
    {
        var movement = StockMovementFactory.CreateInbound(1, 10, "PO-12345");

        movement.ProductId.Should().Be(1);
        movement.Type.Should().Be(MovementType.In);
        movement.Quantity.Should().Be(10);
        movement.Reference.Should().Be("PO-12345");
    }

    [Fact]
    public void CreateInbound_ZeroQuantity_ThrowsDomainException()
    {
        var act = () => StockMovementFactory.CreateInbound(1, 0, "PO-12345");

        act.Should().Throw<DomainException>()
            .WithMessage("*positive*");
    }

    [Fact]
    public void CreateInbound_MissingSupplierRef_ThrowsDomainException()
    {
        var act = () => StockMovementFactory.CreateInbound(1, 10, "  ");

        act.Should().Throw<DomainException>()
            .WithMessage("*supplier reference*");
    }

    [Fact]
    public void CreateOutbound_QuantityExceedsStock_ThrowsInsufficientStockException()
    {
        var act = () => StockMovementFactory.CreateOutbound(1, 15, 10);

        act.Should().Throw<InsufficientStockException>();
    }

    [Fact]
    public void CreateAdjustment_NoReason_ThrowsDomainException()
    {
        var act = () => StockMovementFactory.CreateAdjustment(1, 5, "");

        act.Should().Throw<DomainException>()
            .WithMessage("*reason*");
    }
}
