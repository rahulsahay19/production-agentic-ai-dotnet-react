using NexaStore.Domain.Common;
using NexaStore.Domain.Inventory;

namespace NexaStore.UnitTests.Inventory;

public sealed class InventoryItemTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ReserveAndCommitReservation_UpdatesAvailableAndOnHandQuantities()
    {
        var inventory = new InventoryItem(Guid.NewGuid(), 10, Now);

        inventory.Reserve(3, Now);
        Assert.Equal(7, inventory.QuantityAvailable);
        Assert.Equal(3, inventory.QuantityReserved);

        inventory.CommitReservation(2, Now);
        Assert.Equal(8, inventory.QuantityOnHand);
        Assert.Equal(1, inventory.QuantityReserved);
        Assert.Equal(7, inventory.QuantityAvailable);
    }

    [Fact]
    public void Reserve_WhenQuantityIsUnavailable_Throws()
    {
        var inventory = new InventoryItem(Guid.NewGuid(), 2, Now);

        Assert.Throws<DomainException>(() => inventory.Reserve(3, Now));
    }
}
