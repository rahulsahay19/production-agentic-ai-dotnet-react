using NexaStore.Domain.Cart;
using NexaStore.Domain.Common;

namespace NexaStore.UnitTests.Cart;

public sealed class CartTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AddItem_CalculatesSubtotalAndCombinesSameProduct()
    {
        var cart = new NexaStore.Domain.Cart.Cart(Guid.NewGuid(), Now);
        var productId = Guid.NewGuid();

        cart.AddItem(productId, "NEXA-1", "NexaBook", 125_000m, "INR", 1, 5, Now);
        cart.AddItem(productId, "NEXA-1", "NexaBook", 125_000m, "INR", 2, 5, Now);

        Assert.Single(cart.Items);
        Assert.Equal(3, cart.Items.Single().Quantity);
        Assert.Equal(375_000m, cart.Subtotal);
    }

    [Fact]
    public void AddItem_WhenRequestedQuantityExceedsStock_Throws()
    {
        var cart = new NexaStore.Domain.Cart.Cart(Guid.NewGuid(), Now);

        var exception = Assert.Throws<DomainException>(() =>
            cart.AddItem(Guid.NewGuid(), "NEXA-1", "NexaBook", 100m, "INR", 3, 2, Now));

        Assert.Contains("exceeds available inventory", exception.Message);
    }
}
