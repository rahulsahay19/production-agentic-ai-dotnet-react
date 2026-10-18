using NexaStore.Domain.Common;

namespace NexaStore.Domain.Orders;

public sealed class OrderItem
{
    private OrderItem() { }

    internal OrderItem(Guid orderId, OrderLine line)
    {
        if (line.ProductId == Guid.Empty)
        {
            throw new DomainException("Product is required for an order item.");
        }

        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = line.ProductId;
        Sku = Guard.Required(line.Sku, nameof(Sku), 64);
        ProductName = Guard.Required(line.ProductName, nameof(ProductName), 200);
        UnitPrice = Guard.NonNegative(line.UnitPrice, nameof(UnitPrice));
        Quantity = Guard.Positive(line.Quantity, nameof(Quantity));
    }

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal LineTotal => UnitPrice * Quantity;
}
