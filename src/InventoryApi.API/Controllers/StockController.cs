using InventoryApi.Application.Stock.Commands;
using InventoryApi.Application.Stock.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApi.API.Controllers;

[ApiController]
[Route("api/products/{productId:int}/stock")]
public class StockController : ControllerBase
{
    private readonly ISender _mediator;

    public StockController(ISender mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> UpdateStock(int productId, UpdateStockCommand cmd, CancellationToken ct)
    {
        if (productId != cmd.ProductId)
            return BadRequest("Route productId does not match command productId.");

        var id = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(GetStockLevel), new { productId }, new { id });
    }

    [HttpGet]
    public async Task<IActionResult> GetStockLevel(int productId, CancellationToken ct)
    {
        var level = await _mediator.Send(new GetStockLevelQuery(productId), ct);
        return level is null ? NotFound() : Ok(new { productId, stockLevel = level });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetStockHistory(int productId, CancellationToken ct)
    {
        var history = await _mediator.Send(new GetStockHistoryQuery(productId), ct);
        return history is null ? NotFound() : Ok(history);
    }
}
