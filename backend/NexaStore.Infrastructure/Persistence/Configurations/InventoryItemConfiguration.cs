using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaStore.Domain.Inventory;

namespace NexaStore.Infrastructure.Persistence.Configurations;

internal sealed class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("inventory");
        builder.HasKey(inventory => inventory.Id);
        builder.Ignore(inventory => inventory.QuantityAvailable);
        builder.HasIndex(inventory => inventory.ProductId).IsUnique();
        builder.HasOne<NexaStore.Domain.Catalog.Product>()
            .WithOne()
            .HasForeignKey<InventoryItem>(inventory => inventory.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
