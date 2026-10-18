using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaStore.Domain.Catalog;

namespace NexaStore.Infrastructure.Persistence.Configurations;

internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");
        builder.HasKey(image => image.Id);
        builder.Property(image => image.Url).HasMaxLength(1000).IsRequired();
        builder.Property(image => image.AltText).HasMaxLength(250).IsRequired();
        builder.HasIndex(image => new { image.ProductId, image.DisplayOrder });
        builder.HasIndex(image => new { image.ProductId, image.IsPrimary });
    }
}
