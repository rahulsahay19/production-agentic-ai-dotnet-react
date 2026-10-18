using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaStore.Domain.Cart;

namespace NexaStore.Infrastructure.Persistence.Configurations;

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Sku).HasMaxLength(64).IsRequired();
        builder.Property(item => item.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(item => item.UnitPrice).HasPrecision(18, 2);
        builder.Property(item => item.Currency).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Ignore(item => item.LineTotal);
        builder.HasIndex(item => new { item.CartId, item.ProductId }).IsUnique();
        builder.HasOne<NexaStore.Domain.Catalog.Product>()
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
