namespace NexaStore.Infrastructure.AI
{
    public sealed class AiOptions
    {
        public const string SectionName = "AI";
        public bool Enabled { get; init; } = true;
        public string Provider { get; init; } = AiProviders.OpenAI;
        public AiProviderOptions Providers { get; init; } = new();

        public int RequestTimeoutSeconds { get; set; } = 120;
        public float Temperature { get; init; } = 0.3f;
        public int MaxOutputTokens { get; init; } = 800;

        public AiProviderConfiguration SelectedProvider =>
            string.Equals(Provider, AiProviders.Ollama, StringComparison.OrdinalIgnoreCase)
            ? Providers.Ollama
            : Providers.OpenAI;
        public void Validate()
        {
            if (!Enabled)
            {
                return;
            }
            if(!AiProviders.IsSupported(Provider))
            {
                throw new InvalidOperationException(
                    $"Unsupported AI provider '{Provider}'. Supported providers: {AiProviders.OpenAI}, {AiProviders.Ollama}.");
            }

            var selectedProvider = SelectedProvider;

            if(!Uri.TryCreate(selectedProvider.Endpoint, UriKind.Absolute, out _))
            {
                throw new InvalidOperationException($"AI:Providers:{Provider}:Endpoint must be an absolute URI.");
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(selectedProvider.Model);

            if(string.Equals(Provider, AiProviders.OpenAI, StringComparison.OrdinalIgnoreCase) &&
                 string.IsNullOrWhiteSpace(selectedProvider.ApiKey))
            {
                throw new InvalidOperationException("AI:Providers:OpenAI:ApiKey is required.");
            }
            
            if(RequestTimeoutSeconds is < 10 or > 600)
            {
                throw new InvalidOperationException("AI:RequestTimeoutSeconds must be between 10 and 600");
            }
            if(Temperature is < 0 or > 2)
            {
                throw new InvalidOperationException("AI:Temperature must be between 0 and 2.");
            }
            if(MaxOutputTokens is < 64 or > 4_096)
            {
                throw new InvalidOperationException("AI: MaxOutputTokens must be between 64 and 4096.");
            }
        }
    }
}

public sealed class AiProviderOptions
{
    public AiProviderConfiguration OpenAI { get; init; } = new()
    {
        Endpoint = "https://api.openai.com/v1",
        Model = "gpt-5.6-terra"
    };

    public AiProviderConfiguration Ollama { get; init; } = new()
    {
        Endpoint = "http://localhost:11434",
        Model = "qwen3.5:4b"
    };
}

public sealed class AiProviderConfiguration
{
    public string Endpoint { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string? ApiKey { get; init; }
}

public static class AiProviders
{
    public const string OpenAI = "OpenAI";
    public const string Ollama = "Ollama";

    public static bool IsSupported(string provider) =>
        string.Equals(provider, OpenAI, StringComparison.OrdinalIgnoreCase)
        || string.Equals(provider, Ollama, StringComparison.OrdinalIgnoreCase);
}