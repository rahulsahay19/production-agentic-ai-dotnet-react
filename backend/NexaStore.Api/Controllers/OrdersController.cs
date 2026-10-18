using Microsoft.AspNetCore.Mvc;
using NexaStore.Application.Abstractions;
using NexaStore.Application.Orders;

namespace NexaStore.Api.Controllers;

[ApiController]
[Route("api/orders")]
[Tags("Orders")]
public sealed class OrdersController(IOrderService orders) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<OrderDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<OrderDto>> Create(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var order = await orders.CreateOrderAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<OrderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<OrderDto> GetOrder(Guid id, CancellationToken cancellationToken) =>
        orders.GetOrderAsync(id, cancellationToken);

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType<OrderDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<OrderDto> Cancel(Guid id, CancellationToken cancellationToken) =>
        orders.CancelOrderAsync(id, cancellationToken);
}
