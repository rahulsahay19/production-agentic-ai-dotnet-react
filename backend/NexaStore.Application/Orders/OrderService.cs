using NexaStore.Application.Abstractions;
using NexaStore.Application.Common;
using NexaStore.Domain.Cart;
using NexaStore.Domain.Customers;
using NexaStore.Domain.Inventory;
using NexaStore.Domain.Orders;
using Microsoft.Extensions.Logging;
using NexaStore.Application.Configuration;

namespace NexaStore.Application.Orders;

public sealed class OrderService(
    IOrderRepository orders,
    ICartRepository carts,
    ICustomerRepository customers,
    IInventoryRepository inventory,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    CommerceOptions commerceOptions,
    ILogger<OrderService> logger) : IOrderService
{
    public Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken) =>
        unitOfWork.ExecuteInTransactionAsync(
            innerCancellationToken => CreateOrderCoreAsync(request, innerCancellationToken),
            cancellationToken);

    private async Task<OrderDto> CreateOrderCoreAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        _ = await customers.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), request.CustomerId);
        var cart = await carts.GetByCustomerIdAsync(request.CustomerId, true, cancellationToken)
            ?? throw new NotFoundException(nameof(Cart), request.CustomerId);
        if (cart.Items.Count == 0)
        {
            throw new ApplicationValidationException("An order cannot be created from an empty cart.");
        }

        var now = timeProvider.GetUtcNow();
        foreach (var cartItem in cart.Items)
        {
            var inventoryItem = await inventory.GetByProductIdAsync(cartItem.ProductId, true, cancellationToken)
                ?? throw new NotFoundException(nameof(InventoryItem), cartItem.ProductId);
            inventoryItem.Reduce(cartItem.Quantity, now);
        }

        var shipping = cart.Subtotal >= commerceOptions.FreeShippingThreshold
            ? 0m
            : commerceOptions.StandardShippingAmount;
        var tax = decimal.Round(cart.Subtotal * commerceOptions.TaxRate, 2, MidpointRounding.AwayFromZero);
        var order = Order.Create(
            CreateOrderNumber(now),
            request.CustomerId,
            cart.Items.Select(item => new OrderLine(
                item.ProductId, item.Sku, item.ProductName, item.UnitPrice, item.Quantity)),
            request.ShippingAddress.ToDomain(),
            request.BillingAddress.ToDomain(),
            shipping,
            tax,
            cart.Items.Select(item => item.Currency).Distinct(StringComparer.OrdinalIgnoreCase).Single(),
            now);
        orders.Add(order);
        cart.Clear(now);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Created order {OrderId} with number {OrderNumber} for customer {CustomerId}; total {TotalAmount} {Currency}",
            order.Id,
            order.OrderNumber,
            order.CustomerId,
            order.TotalAmount,
            order.Currency);
        return order.ToDto();
    }

    public async Task<OrderDto> GetOrderAsync(Guid id, CancellationToken cancellationToken) =>
        (await GetOrderEntityAsync(id, cancellationToken)).ToDto();

    public async Task<IReadOnlyCollection<OrderDto>> GetOrdersByCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        _ = await customers.GetByIdAsync(customerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), customerId);
        return (await orders.GetByCustomerIdAsync(customerId, cancellationToken))
            .Select(order => order.ToDto())
            .ToArray();
    }

    public Task<OrderDto> CancelOrderAsync(Guid id, CancellationToken cancellationToken) =>
        unitOfWork.ExecuteInTransactionAsync(
            innerCancellationToken => CancelOrderCoreAsync(id, innerCancellationToken),
            cancellationToken);

    private async Task<OrderDto> CancelOrderCoreAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await GetOrderEntityAsync(id, cancellationToken);
        var now = timeProvider.GetUtcNow();
        order.Cancel(now);
        foreach (var orderItem in order.Items)
        {
            var inventoryItem = await inventory.GetByProductIdAsync(orderItem.ProductId, true, cancellationToken)
                ?? throw new NotFoundException(nameof(InventoryItem), orderItem.ProductId);
            inventoryItem.Restock(orderItem.Quantity, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Cancelled order {OrderId} for customer {CustomerId} and restored {ItemCount} inventory lines",
            order.Id,
            order.CustomerId,
            order.Items.Count);
        return order.ToDto();
    }

    private async Task<Order> GetOrderEntityAsync(Guid id, CancellationToken cancellationToken) =>
        await orders.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(nameof(Order), id);

    private static string CreateOrderNumber(DateTimeOffset now)
    {
        var sortableId = Guid.CreateVersion7(now).ToString("N")[..16].ToUpperInvariant();
        return $"NX-{now:yyyyMMdd}-{sortableId}";
    }
}
