using NexaStore.Domain.Orders;

namespace NexaStore.Application.Orders;

public sealed record AddressDto(
    string RecipientName,
    string Line1,
    string? Line2,
    string City,
    string StateOrProvince,
    string PostalCode,
    string CountryCode,
    string? Phone);

public sealed record CreateOrderRequest(Guid CustomerId, AddressDto ShippingAddress, AddressDto BillingAddress);

public sealed record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    DateTimeOffset OrderDate,
    IReadOnlyCollection<OrderItemDto> Items,
    decimal Subtotal,
    decimal ShippingAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string Currency,
    OrderStatus OrderStatus,
    PaymentStatus PaymentStatus,
    AddressDto ShippingAddress,
    AddressDto BillingAddress,
    DateTimeOffset? CancelledAt);

public sealed record OrderItemDto(
    Guid ProductId,
    string Sku,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);
