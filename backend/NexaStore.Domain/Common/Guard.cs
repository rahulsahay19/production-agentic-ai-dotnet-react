namespace NexaStore.Domain.Common;

internal static class Guard
{
    public static string Required(string? value, string fieldName, int maximumLength = int.MaxValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        var result = value.Trim();
        if (result.Length > maximumLength)
        {
            throw new DomainException($"{fieldName} cannot exceed {maximumLength} characters.");
        }

        return result;
    }

    public static decimal NonNegative(decimal value, string fieldName)
    {
        if (value < 0)
        {
            throw new DomainException($"{fieldName} cannot be negative.");
        }

        return value;
    }

    public static int Positive(int value, string fieldName)
    {
        if (value <= 0)
        {
            throw new DomainException($"{fieldName} must be greater than zero.");
        }

        return value;
    }

    public static int NonNegative(int value, string fieldName)
    {
        if (value < 0)
        {
            throw new DomainException($"{fieldName} cannot be negative.");
        }

        return value;
    }
}
