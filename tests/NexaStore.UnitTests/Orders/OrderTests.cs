using NexaStore.Domain.Common;
using NexaStore.Domain.Customers;
using NexaStore.Domain.Orders;

namespace NexaStore.UnitTests.Orders;

public sealed class OrderTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_CalculatesTotalsFromSnapshottedItems()
    {
        var order = CreateOrder();

        Assert.Equal(250_000m, order.Subtotal);
        Assert.Equal(295_000m, order.TotalAmount);
        Assert.Equal(OrderStatus.Pending, order.OrderStatus);
        Assert.Equal(PaymentStatus.Pending, order.PaymentStatus);
    }

    [Fact]
    public void Cancel_WhenOrderIsProcessing_Throws()
    {
        var order = CreateOrder();
        order.Confirm();
        order.StartProcessing();

        Assert.Throws<DomainException>(() => order.Cancel(Now.AddMinutes(1)));
    }

    [Fact]
    public void Cancel_WhenOrderIsConfirmed_SetsCancellationState()
    {
        var order = CreateOrder();
        order.Confirm();

        order.Cancel(Now.AddMinutes(1));

        Assert.Equal(OrderStatus.Cancelled, order.OrderStatus);
        Assert.Equal(Now.AddMinutes(1), order.CancelledAt);
    }

    private static Order CreateOrder()
    {
        var address = new Address("Rahul Sahay", "1 Technology Park", null, "Pune", "Maharashtra", "411001", "IN");
        var lines = new[]
        {
            new OrderLine(Guid.NewGuid(), "NEXA-1", "NexaBook", 125_000m, 2)
        };

        return Order.Create("NX-2026-000001", Guid.NewGuid(), lines, address, address, 5_000m, 40_000m, "INR", Now);
    }
}
