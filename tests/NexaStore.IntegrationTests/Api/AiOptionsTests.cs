using NexaStore.Infrastructure.AI;

namespace NexaStore.IntegrationTests.Api;

public sealed class AiOptionsTests
{
    [Fact]
    public void Validate_AcceptsOpenAiWithServerCredential()
    {
        var options = new AiOptions
        {
            Provider = AiProviders.OpenAI,
            Providers = CreateProviders(openAiApiKey: "test-key")
        };

        options.Validate();
    }

    [Fact]
    public void Validate_RejectsOpenAiWithoutServerCredential()
    {
        var options = new AiOptions
        {
            Provider = AiProviders.OpenAI,
            Providers = CreateProviders()
        };

        var exception = Assert.Throws<InvalidOperationException>(options.Validate);

        Assert.Contains("AI:Providers:OpenAI:ApiKey", exception.Message);
    }

    [Fact]
    public void Validate_AcceptsOllamaWithoutApiKey()
    {
        var options = new AiOptions
        {
            Provider = AiProviders.Ollama,
            Providers = CreateProviders()
        };

        options.Validate();
    }

    [Fact]
    public void Validate_RejectsUnknownProvider()
    {
        var options = new AiOptions
        {
            Provider = "Unknown",
            Providers = CreateProviders()
        };

        Assert.Throws<InvalidOperationException>(options.Validate);
    }

    private static AiProviderOptions CreateProviders(string? openAiApiKey = null) => new()
    {
        OpenAI = new AiProviderConfiguration
        {
            Endpoint = "https://api.openai.com/v1",
            Model = "gpt-5.6-terra",
            ApiKey = openAiApiKey
        },
        Ollama = new AiProviderConfiguration
        {
            Endpoint = "http://localhost:11434",
            Model = "qwen3.5:4b"
        }
    };
}
