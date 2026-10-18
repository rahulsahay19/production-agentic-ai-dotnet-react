using NexaStore.Domain.Common;

namespace NexaStore.Domain.Cart;

public sealed class Cart
{
    private readonly List<CartItem> _items = [];

    private Cart() { }

    public Cart(Guid customerId, DateTimeOffset createdAt)
    {
        if (customerId == Guid.Empty)
        {
            throw new DomainException("Customer is required.");
        }

        Id = Guid.NewGuid();
        CustomerId = customerId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
    public decimal Subtotal => _items.Sum(item => item.LineTotal);

    public void AddItem(
        Guid productId,
        string sku,
        string productName,
        decimal unitPrice,
        string currency,
        int quantity,
        int availableQuantity,
        DateTimeOffset updatedAt)
    {
        Guard.Positive(quantity, nameof(quantity));
        Guard.NonNegative(availableQuantity, nameof(availableQuantity));
        var existingItem = _items.SingleOrDefault(item => item.ProductId == productId);
        var requestedTotal = quantity + (existingItem?.Quantity ?? 0);
        EnsureAvailable(requestedTotal, availableQuantity);

        if (existingItem is null)
        {
            _items.Add(new CartItem(Id, productId, sku, productName, unitPrice, currency, quantity));
        }
        else
        {
            existingItem.SetQuantity(requestedTotal);
            existingItem.RefreshPrice(unitPrice, currency);
        }

        UpdatedAt = updatedAt;
    }

    public void UpdateItem(Guid productId, int quantity, int availableQuantity, DateTimeOffset updatedAt)
    {
        Guard.Positive(quantity, nameof(quantity));
        EnsureAvailable(quantity, availableQuantity);
        GetItem(productId).SetQuantity(quantity);
        UpdatedAt = updatedAt;
    }

    public void RemoveItem(Guid productId, DateTimeOffset updatedAt)
    {
        var item = GetItem(productId);
        _items.Remove(item);
        UpdatedAt = updatedAt;
    }

    public void Clear(DateTimeOffset updatedAt)
    {
        _items.Clear();
        UpdatedAt = updatedAt;
    }

    private CartItem GetItem(Guid productId) =>
        _items.SingleOrDefault(item => item.ProductId == productId)
        ?? throw new DomainException("The product is not in the cart.");

    private static void EnsureAvailable(int requested, int available)
    {
        if (requested > available)
        {
            throw new DomainException("The requested quantity exceeds available inventory.");
        }
    }
}
