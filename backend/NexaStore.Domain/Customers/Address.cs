using NexaStore.Domain.Common;

namespace NexaStore.Domain.Customers;

public sealed record Address
{
    private Address() { }

    public Address(
        string recipientName,
        string line1,
        string? line2,
        string city,
        string stateOrProvince,
        string postalCode,
        string countryCode,
        string? phone = null)
    {
        RecipientName = Guard.Required(recipientName, nameof(RecipientName), 150);
        Line1 = Guard.Required(line1, nameof(Line1), 200);
        Line2 = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim();
        City = Guard.Required(city, nameof(City), 100);
        StateOrProvince = Guard.Required(stateOrProvince, nameof(StateOrProvince), 100);
        PostalCode = Guard.Required(postalCode, nameof(PostalCode), 20);
        CountryCode = Guard.Required(countryCode, nameof(CountryCode), 2).ToUpperInvariant();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
    }

    public string RecipientName { get; private init; } = string.Empty;
    public string Line1 { get; private init; } = string.Empty;
    public string? Line2 { get; private init; }
    public string City { get; private init; } = string.Empty;
    public string StateOrProvince { get; private init; } = string.Empty;
    public string PostalCode { get; private init; } = string.Empty;
    public string CountryCode { get; private init; } = string.Empty;
    public string? Phone { get; private init; }
}
