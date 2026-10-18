using Microsoft.AspNetCore.Mvc;
using NexaStore.Application.Abstractions;
using NexaStore.Application.Inventory;

namespace NexaStore.Api.Controllers;

[ApiController]
[Route("api/inventory/{productId:guid}")]
[Tags("Inventory")]
public sealed class InventoryController(IInventoryService inventory) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<InventoryDto>(StatusCodes.Status200OK)]
    public Task<InventoryDto> GetInventory(Guid productId, CancellationToken cancellationToken) =>
        inventory.GetInventoryAsync(productId, cancellationToken);

    [HttpGet("availability")]
    [ProducesResponseType<InventoryAvailabilityDto>(StatusCodes.Status200OK)]
    public Task<InventoryAvailabilityDto> CheckAvailability(
        Guid productId,
        [FromQuery] int quantity = 1,
        CancellationToken cancellationToken = default) =>
        inventory.CheckAvailabilityAsync(productId, quantity, cancellationToken);
}
