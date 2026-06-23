using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using InventoryApi.Application.DTOs;

namespace InventoryApi.IntegrationTests.Controllers;

public class CategoriesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CategoriesControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateCategory_ValidRequest_Returns201Created()
    {
        var response = await _client.PostAsJsonAsync("/api/categories", new { name = "Electronics" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        body!["id"].Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllCategories_Returns200OK()
    {
        await _client.PostAsJsonAsync("/api/categories", new { name = "Office Supplies" });

        var response = await _client.GetAsync("/api/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryDto>>();
        categories.Should().NotBeEmpty();
        categories!.Should().Contain(c => c.Name == "Office Supplies");
    }
}
