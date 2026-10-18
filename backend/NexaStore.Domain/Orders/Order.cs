using NexaStore.Domain.Common;
using NexaStore.Domain.Customers;

namespace NexaStore.Domain.Orders;

public sealed class Order
{
    private readonly List<OrderItem> _items = [];

    private Order() { }

    public static Order Create(
        string orderNumber,
        Guid customerId,
        IEnumerable<OrderLine> lines,
        Address shippingAddress,
        Address billingAddress,
        decimal shippingAmount,
        decimal taxAmount,
        string currency,
        DateTimeOffset orderDate)
    {
        var orderLines = lines?.ToArray() ?? throw new ArgumentNullException(nameof(lines));
        if (orderLines.Length == 0)
        {
            throw new DomainException("An order must contain at least one item.");
        }

        if (customerId == Guid.Empty)
        {
            throw new DomainException("Customer is required.");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = Guard.Required(orderNumber, nameof(OrderNumber), 50),
            CustomerId = customerId,
            OrderDate = orderDate,
            ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress)),
            BillingAddress = billingAddress ?? throw new ArgumentNullException(nameof(billingAddress)),
            ShippingAmount = Guard.NonNegative(shippingAmount, nameof(ShippingAmount)),
            TaxAmount = Guard.NonNegative(taxAmount, nameof(TaxAmount)),
            Currency = Guard.Required(currency, nameof(Currency), 3).ToUpperInvariant(),
            OrderStatus = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending
        };

        foreach (var line in orderLines)
        {
            order._items.Add(new OrderItem(order.Id, line));
        }

        return order;
    }

    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public DateTimeOffset OrderDate { get; private set; }
    public Address ShippingAddress { get; private set; } = null!;
    public Address BillingAddress { get; private set; } = null!;
    public decimal Subtotal => _items.Sum(item => item.LineTotal);
    public decimal ShippingAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal TotalAmount => Subtotal + ShippingAmount + TaxAmount;
    public string Currency { get; private set; } = string.Empty;
    public OrderStatus OrderStatus { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public uint Version { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public void Confirm() => TransitionTo(OrderStatus.Confirmed, OrderStatus.Pending);

    public void StartProcessing() => TransitionTo(OrderStatus.Processing, OrderStatus.Confirmed);

    public void MarkShipped() => TransitionTo(OrderStatus.Shipped, OrderStatus.Processing);

    public void MarkDelivered() => TransitionTo(OrderStatus.Delivered, OrderStatus.Shipped);

    public void MarkReturned()
    {
        if (OrderStatus != OrderStatus.Delivered)
        {
            throw new DomainException("Only a delivered order can be marked as returned.");
        }

        OrderStatus = OrderStatus.Returned;
    }

    public void Cancel(DateTimeOffset cancelledAt)
    {
        if (OrderStatus is not (OrderStatus.Pending or OrderStatus.Confirmed))
        {
            throw new DomainException($"An order in {OrderStatus} status cannot be cancelled.");
        }

        OrderStatus = OrderStatus.Cancelled;
        CancelledAt = cancelledAt;
    }

    public void SetPaymentStatus(PaymentStatus paymentStatus)
    {
        if (paymentStatus == PaymentStatus.Refunded && PaymentStatus != PaymentStatus.Paid)
        {
            throw new DomainException("Only a paid order can be marked as refunded.");
        }

        PaymentStatus = paymentStatus;
    }

    private void TransitionTo(OrderStatus next, OrderStatus requiredCurrent)
    {
        if (OrderStatus != requiredCurrent)
        {
            throw new DomainException($"Order must be {requiredCurrent} before it can become {next}.");
        }

        OrderStatus = next;
    }
}
