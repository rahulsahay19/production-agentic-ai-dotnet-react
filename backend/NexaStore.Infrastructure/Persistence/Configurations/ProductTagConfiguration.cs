using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaStore.Domain.Catalog;

namespace NexaStore.Infrastructure.Persistence.Configurations;

internal sealed class ProductTagConfiguration : IEntityTypeConfiguration<ProductTag>
{
    public void Configure(EntityTypeBuilder<ProductTag> builder)
    {
        builder.ToTable("product_tags");
        builder.HasKey(tag => tag.Id);
        builder.Property(tag => tag.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(tag => new { tag.ProductId, tag.Name }).IsUnique();
    }
}
