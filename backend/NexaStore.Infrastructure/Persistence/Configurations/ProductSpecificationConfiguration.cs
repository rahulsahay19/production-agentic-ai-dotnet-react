using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaStore.Domain.Catalog;

namespace NexaStore.Infrastructure.Persistence.Configurations;

internal sealed class ProductSpecificationConfiguration : IEntityTypeConfiguration<ProductSpecification>
{
    public void Configure(EntityTypeBuilder<ProductSpecification> builder)
    {
        builder.ToTable("product_specifications");
        builder.HasKey(specification => specification.Id);
        builder.Property(specification => specification.Group).HasMaxLength(100).IsRequired();
        builder.Property(specification => specification.Name).HasMaxLength(100).IsRequired();
        builder.Property(specification => specification.Value).HasMaxLength(500).IsRequired();
        builder.HasIndex(specification => new { specification.ProductId, specification.Group });
    }
}
