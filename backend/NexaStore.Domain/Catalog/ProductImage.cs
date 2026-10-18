using NexaStore.Domain.Common;

namespace NexaStore.Domain.Catalog;

public sealed class ProductImage
{
    private ProductImage() { }

    internal ProductImage(Guid productId, string url, string altText, bool isPrimary, int displayOrder)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        Url = Guard.Required(url, nameof(Url), 1000);
        AltText = Guard.Required(altText, nameof(AltText), 250);
        IsPrimary = isPrimary;
        DisplayOrder = Guard.NonNegative(displayOrder, nameof(DisplayOrder));
    }

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string AltText { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }
    public int DisplayOrder { get; private set; }

    internal void ClearPrimary() => IsPrimary = false;
}
