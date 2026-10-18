namespace NexaStore.Application.Configuration;

public sealed class CommerceOptions
{
    public const string SectionName = "Commerce";
    public decimal TaxRate { get; init; } = 0.18m;
    public decimal FreeShippingThreshold { get; init; } = 100_000m;
    public decimal StandardShippingAmount { get; init; } = 999m;

    public void Validate()
    {
        if (TaxRate is < 0 or > 1)
        {
            throw new InvalidOperationException("Commerce tax rate must be between 0 and 1.");
        }

        if (FreeShippingThreshold < 0 || StandardShippingAmount < 0)
        {
            throw new InvalidOperationException("Commerce shipping values cannot be negative.");
        }
    }
}
