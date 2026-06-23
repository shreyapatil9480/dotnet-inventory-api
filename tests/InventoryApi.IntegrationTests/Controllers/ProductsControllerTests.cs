using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InventoryApi.Application.DTOs;

namespace InventoryApi.IntegrationTests.Controllers;

public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<int> CreateCategoryAsync(string name = "Test Category")
    {
        var response = await _client.PostAsJsonAsync("/api/categories", new { name });
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        return body!["id"];
    }

    [Fact]
    public async Task CreateProduct_ValidRequest_Returns201Created()
    {
        var categoryId = await CreateCategoryAsync();

        var response = await _client.PostAsJsonAsync("/api/products", new
        {
            name = "Widget A",
            sku = "WGT-001",
            price = 9.99m,
            categoryId
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        body!["id"].Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetProductById_ExistingId_Returns200OK()
    {
        var categoryId = await CreateCategoryAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/products", new
        {
            name = "Widget B",
            sku = "WGT-002",
            price = 14.99m,
            categoryId
        });
        var created = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        var id = created!["id"];

        var response = await _client.GetAsync($"/api/products/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<ProductDto>();
        product!.Name.Should().Be("Widget B");
        product.SKU.Should().Be("WGT-002");
    }

    [Fact]
    public async Task GetProductById_NonExistentId_Returns404NotFound()
    {
        var response = await _client.GetAsync("/api/products/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllProducts_Returns200OK()
    {
        var categoryId = await CreateCategoryAsync();
        await _client.PostAsJsonAsync("/api/products", new
        {
            name = "Widget C",
            sku = "WGT-003",
            price = 5.99m,
            categoryId
        });

        var response = await _client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>();
        products.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateProduct_ValidRequest_Returns204NoContent()
    {
        var categoryId = await CreateCategoryAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/products", new
        {
            name = "Widget D",
            sku = "WGT-004",
            price = 10m,
            categoryId
        });
        var created = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        var id = created!["id"];

        var response = await _client.PutAsJsonAsync($"/api/products/{id}", new
        {
            id,
            name = "Widget D Updated",
            sku = "WGT-004",
            price = 12m,
            categoryId
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/products/{id}");
        var product = await getResponse.Content.ReadFromJsonAsync<ProductDto>();
        product!.Name.Should().Be("Widget D Updated");
        product.Price.Should().Be(12m);
    }

    [Fact]
    public async Task DeleteProduct_ExistingId_Returns204NoContent()
    {
        var categoryId = await CreateCategoryAsync();
        var createResponse = await _client.PostAsJsonAsync("/api/products", new
        {
            name = "Widget E",
            sku = "WGT-005",
            price = 8m,
            categoryId
        });
        var created = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        var id = created!["id"];

        var response = await _client.DeleteAsync($"/api/products/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/products/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
