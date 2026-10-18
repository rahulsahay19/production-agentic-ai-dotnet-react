using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexaStore.Domain.Customers;

namespace NexaStore.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(customer => customer.Id);
        builder.Property(customer => customer.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.LastName).HasMaxLength(100).IsRequired();
        builder.Property(customer => customer.Email).HasMaxLength(320).IsRequired();
        builder.Property(customer => customer.Phone).HasMaxLength(30);
        builder.HasIndex(customer => customer.Email).IsUnique();

        ConfigureAddress(builder.OwnsOne(customer => customer.ShippingAddress), "shipping");
        ConfigureAddress(builder.OwnsOne(customer => customer.BillingAddress), "billing");
    }

    private static void ConfigureAddress(OwnedNavigationBuilder<Customer, Address> builder, string prefix)
    {
        builder.Property(address => address.RecipientName).HasColumnName($"{prefix}_recipient_name").HasMaxLength(150);
        builder.Property(address => address.Line1).HasColumnName($"{prefix}_line1").HasMaxLength(200);
        builder.Property(address => address.Line2).HasColumnName($"{prefix}_line2").HasMaxLength(200);
        builder.Property(address => address.City).HasColumnName($"{prefix}_city").HasMaxLength(100);
        builder.Property(address => address.StateOrProvince).HasColumnName($"{prefix}_state_or_province").HasMaxLength(100);
        builder.Property(address => address.PostalCode).HasColumnName($"{prefix}_postal_code").HasMaxLength(20);
        builder.Property(address => address.CountryCode).HasColumnName($"{prefix}_country_code").HasMaxLength(2);
        builder.Property(address => address.Phone).HasColumnName($"{prefix}_phone").HasMaxLength(30);
    }
}
