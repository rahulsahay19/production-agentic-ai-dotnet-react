using NexaStore.Domain.Common;

namespace NexaStore.Domain.Catalog;

public sealed class Category
{
    private Category() { }

    public Category(string name, string slug, string? description = null)
    {
        Id = Guid.NewGuid();
        Name = Guard.Required(name, nameof(Name), 100);
        Slug = Guard.Required(slug, nameof(Slug), 120).ToLowerInvariant();
        Description = description?.Trim();
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
}
