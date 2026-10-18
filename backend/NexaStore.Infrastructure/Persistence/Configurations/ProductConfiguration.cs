using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaStore.Domain.Catalog;

namespace NexaStore.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Sku).HasMaxLength(64).IsRequired();
        builder.Property(product => product.Name).HasMaxLength(200).IsRequired();
        builder.Property(product => product.Slug).HasMaxLength(220).IsRequired();
        builder.Property(product => product.Brand).HasMaxLength(100).IsRequired();
        builder.Property(product => product.ShortDescription).HasMaxLength(500).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(5000).IsRequired();
        builder.Property(product => product.Price).HasPrecision(18, 2);
        builder.Property(product => product.Currency).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(product => product.Cpu).HasMaxLength(200);
        builder.Property(product => product.Gpu).HasMaxLength(200);
        builder.Property(product => product.Npu).HasMaxLength(200);
        builder.Property(product => product.StorageType).HasMaxLength(100);
        builder.Property(product => product.Display).HasMaxLength(300);
        builder.Property(product => product.BatteryWh).HasPrecision(8, 2);
        builder.Property(product => product.WeightKg).HasPrecision(8, 3);

        builder.HasIndex(product => product.Sku).IsUnique();
        builder.HasIndex(product => product.Slug).IsUnique();
        builder.HasIndex(product => new { product.CategoryId, product.IsActive });
        builder.HasIndex(product => new { product.IsFeatured, product.IsActive });
        builder.HasIndex(product => product.Brand);
        builder.HasIndex(product => product.Price);
        builder.HasIndex(product => new { product.RamGb, product.VramGb });

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(product => product.Images)
            .WithOne()
            .HasForeignKey(image => image.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(product => product.Specifications)
            .WithOne()
            .HasForeignKey(specification => specification.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(product => product.Tags)
            .WithOne()
            .HasForeignKey(tag => tag.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(product => product.Images).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(product => product.Specifications).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(product => product.Tags).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
