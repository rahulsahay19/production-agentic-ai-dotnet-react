using NexaStore.Domain.Common;

namespace NexaStore.Domain.Inventory;

public sealed class InventoryItem
{
    private InventoryItem() { }

    public InventoryItem(Guid productId, int quantityOnHand, DateTimeOffset updatedAt)
    {
        if (productId == Guid.Empty)
        {
            throw new DomainException("Product is required.");
        }

        Id = Guid.NewGuid();
        ProductId = productId;
        QuantityOnHand = Guard.NonNegative(quantityOnHand, nameof(QuantityOnHand));
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public int QuantityOnHand { get; private set; }
    public int QuantityReserved { get; private set; }
    public int QuantityAvailable => QuantityOnHand - QuantityReserved;
    public DateTimeOffset UpdatedAt { get; private set; }
    public uint Version { get; private set; }

    public bool IsAvailable(int quantity) => quantity > 0 && QuantityAvailable >= quantity;

    public void Reserve(int quantity, DateTimeOffset updatedAt)
    {
        Guard.Positive(quantity, nameof(quantity));
        if (!IsAvailable(quantity))
        {
            throw new DomainException("Insufficient inventory is available to reserve the requested quantity.");
        }

        QuantityReserved += quantity;
        UpdatedAt = updatedAt;
    }

    public void Release(int quantity, DateTimeOffset updatedAt)
    {
        Guard.Positive(quantity, nameof(quantity));
        if (quantity > QuantityReserved)
        {
            throw new DomainException("Cannot release more stock than is currently reserved.");
        }

        QuantityReserved -= quantity;
        UpdatedAt = updatedAt;
    }

    public void CommitReservation(int quantity, DateTimeOffset updatedAt)
    {
        Guard.Positive(quantity, nameof(quantity));
        if (quantity > QuantityReserved)
        {
            throw new DomainException("Cannot commit more stock than is currently reserved.");
        }

        QuantityReserved -= quantity;
        QuantityOnHand -= quantity;
        UpdatedAt = updatedAt;
    }

    public void Reduce(int quantity, DateTimeOffset updatedAt)
    {
        Guard.Positive(quantity, nameof(quantity));
        if (quantity > QuantityAvailable)
        {
            throw new DomainException("Insufficient unreserved inventory to reduce stock.");
        }

        QuantityOnHand -= quantity;
        UpdatedAt = updatedAt;
    }

    public void Restock(int quantity, DateTimeOffset updatedAt)
    {
        QuantityOnHand += Guard.Positive(quantity, nameof(quantity));
        UpdatedAt = updatedAt;
    }
}
