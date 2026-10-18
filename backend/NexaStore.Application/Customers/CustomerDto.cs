using NexaStore.Application.Orders;

namespace NexaStore.Application.Customers;

public sealed record CustomerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    AddressDto? ShippingAddress,
    AddressDto? BillingAddress,
    DateTimeOffset CreatedAt);
