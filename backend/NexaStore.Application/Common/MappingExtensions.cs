using NexaStore.Application.Cart;
using NexaStore.Application.Customers;
using NexaStore.Application.Orders;
using NexaStore.Domain.Customers;
using NexaStore.Domain.Orders;

namespace NexaStore.Application.Common;

internal static class MappingExtensions
{
    public static CartDto ToDto(this NexaStore.Domain.Cart.Cart cart)
    {
        var currency = cart.Items.Select(item => item.Currency).Distinct(StringComparer.OrdinalIgnoreCase).SingleOrDefault() ?? "INR";
        return new CartDto(
            cart.Id,
            cart.CustomerId,
            cart.Items.Select(item => new CartItemDto(
                item.ProductId,
                item.Sku,
                item.ProductName,
                item.UnitPrice,
                item.Currency,
                item.Quantity,
                item.LineTotal)).ToArray(),
            cart.Subtotal,
            currency);
    }

    public static OrderDto ToDto(this Order order) => new(
        order.Id,
        order.OrderNumber,
        order.CustomerId,
        order.OrderDate,
        order.Items.Select(item => new OrderItemDto(
            item.ProductId,
            item.Sku,
            item.ProductName,
            item.UnitPrice,
            item.Quantity,
            item.LineTotal)).ToArray(),
        order.Subtotal,
        order.ShippingAmount,
        order.TaxAmount,
        order.TotalAmount,
        order.Currency,
        order.OrderStatus,
        order.PaymentStatus,
        order.ShippingAddress.ToDto(),
        order.BillingAddress.ToDto(),
        order.CancelledAt);

    public static CustomerDto ToDto(this Customer customer) => new(
        customer.Id,
        customer.FirstName,
        customer.LastName,
        customer.Email,
        customer.Phone,
        customer.ShippingAddress?.ToDto(),
        customer.BillingAddress?.ToDto(),
        customer.CreatedAt);

    public static AddressDto ToDto(this Address address) => new(
        address.RecipientName,
        address.Line1,
        address.Line2,
        address.City,
        address.StateOrProvince,
        address.PostalCode,
        address.CountryCode,
        address.Phone);

    public static Address ToDomain(this AddressDto address) => new(
        address.RecipientName,
        address.Line1,
        address.Line2,
        address.City,
        address.StateOrProvince,
        address.PostalCode,
        address.CountryCode,
        address.Phone);
}
