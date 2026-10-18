using Microsoft.AspNetCore.Mvc;
using NexaStore.Application.Abstractions;
using NexaStore.Application.Cart;

namespace NexaStore.Api.Controllers;

[ApiController]
[Route("api/cart/{customerId:guid}")]
[Tags("Cart")]
public sealed class CartController(ICartService cart) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<CartDto>(StatusCodes.Status200OK)]
    public Task<CartDto> GetCart(Guid customerId, CancellationToken cancellationToken) =>
        cart.GetCartAsync(customerId, cancellationToken);

    [HttpPost("items")]
    [ProducesResponseType<CartDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<CartDto> AddItem(
        Guid customerId,
        [FromBody] AddCartItemRequest request,
        CancellationToken cancellationToken) =>
        cart.AddItemAsync(customerId, request, cancellationToken);

    [HttpPut("items/{productId:guid}")]
    [ProducesResponseType<CartDto>(StatusCodes.Status200OK)]
    public Task<CartDto> UpdateItem(
        Guid customerId,
        Guid productId,
        [FromBody] UpdateCartItemRequest request,
        CancellationToken cancellationToken) =>
        cart.UpdateItemAsync(customerId, productId, request, cancellationToken);

    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveItem(Guid customerId, Guid productId, CancellationToken cancellationToken)
    {
        await cart.RemoveItemAsync(customerId, productId, cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Clear(Guid customerId, CancellationToken cancellationToken)
    {
        await cart.ClearAsync(customerId, cancellationToken);
        return NoContent();
    }
}
