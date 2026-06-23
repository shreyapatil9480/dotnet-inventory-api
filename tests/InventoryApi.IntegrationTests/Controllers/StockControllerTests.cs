using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InventoryApi.Application.DTOs;
using InventoryApi.Domain.Enums;

namespace InventoryApi.IntegrationTests.Controllers;

public class StockControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public StockControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<int> CreateProductAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        var categoryResponse = await _client.PostAsJsonAsync("/api/categories", new { name = $"Stock Category {suffix}" });
        categoryResponse.EnsureSuccessStatusCode();
        var category = await categoryResponse.Content.ReadFromJsonAsync<Dictionary<string, int>>();

        var productResponse = await _client.PostAsJsonAsync("/api/products", new
        {
            name = "Stock Widget",
            sku = $"STK-{suffix}",
            price = 10m,
            categoryId = category!["id"]
        });
        productResponse.EnsureSuccessStatusCode();
        var product = await productResponse.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        return product!["id"];
    }

    [Fact]
    public async Task UpdateStock_Inbound_Returns201Created()
    {
        var productId = await CreateProductAsync();

        var response = await _client.PostAsJsonAsync($"/api/products/{productId}/stock", new
        {
            productId,
            type = MovementType.In,
            quantity = 50,
            reference = "PO-1001"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetStockLevel_AfterInbound_Returns200OK()
    {
        var productId = await CreateProductAsync();
        await _client.PostAsJsonAsync($"/api/products/{productId}/stock", new
        {
            productId,
            type = MovementType.In,
            quantity = 30,
            reference = "PO-1002"
        });

        var response = await _client.GetAsync($"/api/products/{productId}/stock");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        body!["stockLevel"].Should().Be(30);
    }

    [Fact]
    public async Task GetStockHistory_Returns200OK()
    {
        var productId = await CreateProductAsync();
        await _client.PostAsJsonAsync($"/api/products/{productId}/stock", new
        {
            productId,
            type = MovementType.In,
            quantity = 20,
            reference = "PO-1003"
        });

        var response = await _client.GetAsync($"/api/products/{productId}/stock/history");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var history = await response.Content.ReadFromJsonAsync<List<StockMovementDto>>();
        history.Should().NotBeEmpty();
        history!.Should().Contain(m => m.Type == MovementType.In && m.Quantity == 20);
    }

    [Fact]
    public async Task UpdateStock_OutboundExceedsStock_Returns409Conflict()
    {
        var productId = await CreateProductAsync();
        await _client.PostAsJsonAsync($"/api/products/{productId}/stock", new
        {
            productId,
            type = MovementType.In,
            quantity = 5,
            reference = "PO-1004"
        });

        var response = await _client.PostAsJsonAsync($"/api/products/{productId}/stock", new
        {
            productId,
            type = MovementType.Out,
            quantity = 10
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetStockLevel_NonExistentProduct_Returns404NotFound()
    {
        var response = await _client.GetAsync("/api/products/99999/stock");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
