namespace NexaStore.Domain.Catalog;

public sealed class ProductTag
{
    private ProductTag() { }

    internal ProductTag(Guid productId, string name)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Name = name;
    }

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string Name { get; private set; } = string.Empty;
}
