using System.Net.Mail;
using NexaStore.Domain.Common;

namespace NexaStore.Domain.Customers;

public sealed class Customer
{
    private Customer() { }

    public Customer(string firstName, string lastName, string email, string? phone, DateTimeOffset createdAt)
    {
        Id = Guid.NewGuid();
        FirstName = Guard.Required(firstName, nameof(FirstName), 100);
        LastName = Guard.Required(lastName, nameof(LastName), 100);
        Email = NormalizeEmail(email);
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public Address? ShippingAddress { get; private set; }
    public Address? BillingAddress { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public void SetShippingAddress(Address address) =>
        ShippingAddress = address ?? throw new ArgumentNullException(nameof(address));

    public void SetBillingAddress(Address address) =>
        BillingAddress = address ?? throw new ArgumentNullException(nameof(address));

    private static string NormalizeEmail(string email)
    {
        var normalized = Guard.Required(email, nameof(Email), 320).ToLowerInvariant();
        try
        {
            return new MailAddress(normalized).Address;
        }
        catch (FormatException exception)
        {
            throw new DomainException($"Email is invalid: {exception.Message}");
        }
    }
}
