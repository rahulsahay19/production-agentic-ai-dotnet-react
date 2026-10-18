namespace NexaStore.Api.Configuration;

public sealed class FrontendOptions
{
    public const string SectionName = "Frontend";
    public string[] AllowedOrigins { get; init; } = [];
}
