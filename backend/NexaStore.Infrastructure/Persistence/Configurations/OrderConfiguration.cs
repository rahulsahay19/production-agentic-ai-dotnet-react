using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaStore.Domain.Customers;
using NexaStore.Domain.Orders;

namespace NexaStore.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(order => order.ShippingAmount).HasPrecision(18, 2);
        builder.Property(order => order.TaxAmount).HasPrecision(18, 2);
        builder.Property(order => order.Currency).HasMaxLength(3).IsFixedLength().IsRequired();
        builder.Property(order => order.OrderStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(order => order.PaymentStatus).HasConversion<string>().HasMaxLength(30);
        builder.Ignore(order => order.Subtotal);
        builder.Ignore(order => order.TotalAmount);
        builder.HasIndex(order => order.OrderNumber).IsUnique();
        builder.HasIndex(order => new { order.CustomerId, order.OrderDate });
        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(order => order.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        ConfigureAddress(builder.OwnsOne(order => order.ShippingAddress), "shipping");
        ConfigureAddress(builder.OwnsOne(order => order.BillingAddress), "billing");
    }

    private static void ConfigureAddress(OwnedNavigationBuilder<Order, Address> builder, string prefix)
    {
        builder.Property(address => address.RecipientName).HasColumnName($"{prefix}_recipient_name").HasMaxLength(150).IsRequired();
        builder.Property(address => address.Line1).HasColumnName($"{prefix}_line1").HasMaxLength(200).IsRequired();
        builder.Property(address => address.Line2).HasColumnName($"{prefix}_line2").HasMaxLength(200);
        builder.Property(address => address.City).HasColumnName($"{prefix}_city").HasMaxLength(100).IsRequired();
        builder.Property(address => address.StateOrProvince).HasColumnName($"{prefix}_state_or_province").HasMaxLength(100).IsRequired();
        builder.Property(address => address.PostalCode).HasColumnName($"{prefix}_postal_code").HasMaxLength(20).IsRequired();
        builder.Property(address => address.CountryCode).HasColumnName($"{prefix}_country_code").HasMaxLength(2).IsRequired();
        builder.Property(address => address.Phone).HasColumnName($"{prefix}_phone").HasMaxLength(30);
    }
}
