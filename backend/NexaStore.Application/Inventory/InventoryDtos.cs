namespace NexaStore.Application.Inventory;

public sealed record InventoryDto(
    Guid ProductId,
    int QuantityOnHand,
    int QuantityReserved,
    int QuantityAvailable,
    DateTimeOffset UpdatedAt);

public sealed record InventoryAvailabilityDto(Guid ProductId, int RequestedQuantity, bool IsAvailable, int QuantityAvailable);
