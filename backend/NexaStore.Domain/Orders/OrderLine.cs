namespace NexaStore.Domain.Orders;

public sealed record OrderLine(
    Guid ProductId,
    string Sku,
    string ProductName,
    decimal UnitPrice,
    int Quantity);
