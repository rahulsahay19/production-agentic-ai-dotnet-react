namespace NexaStore.Application.Cart;

public sealed record CartDto(
    Guid Id,
    Guid CustomerId,
    IReadOnlyCollection<CartItemDto> Items,
    decimal Subtotal,
    string Currency);

public sealed record CartItemDto(
    Guid ProductId,
    string Sku,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal LineTotal);

public sealed record AddCartItemRequest(Guid ProductId, int Quantity);
public sealed record UpdateCartItemRequest(int Quantity);
