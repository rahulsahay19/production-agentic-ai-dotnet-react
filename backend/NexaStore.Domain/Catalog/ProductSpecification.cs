using NexaStore.Domain.Common;

namespace NexaStore.Domain.Catalog;

public sealed class ProductSpecification
{
    private ProductSpecification() { }

    internal ProductSpecification(Guid productId, string group, string name, string value, int displayOrder)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Group = Guard.Required(group, nameof(Group), 100);
        Name = Guard.Required(name, nameof(Name), 100);
        Value = Guard.Required(value, nameof(Value), 500);
        DisplayOrder = Guard.NonNegative(displayOrder, nameof(DisplayOrder));
    }

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string Group { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
}
