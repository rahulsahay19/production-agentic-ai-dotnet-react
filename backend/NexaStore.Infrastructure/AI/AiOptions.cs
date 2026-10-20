namespace NexaStore.Infrastructure.AI
{
    public sealed class AiOptions
    {
        public const string SectionName = "AI";
        public bool Enabled { get; init; } = true;
        public string Provider { get; init; } = "ollama";
        public string Endpoint { get; init; } = "http://localhost:11434";
        public string Model { get; init; } = "qwen3.5:4b";

        public void Validate()
        {
            if (!Enabled)
            {
                return;
            }
            if(!string.Equals(Provider, "Ollama", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Unsupported AI provider '{Provider}'. Phase 02 supports only Ollama");
            }
            if(!Uri.TryCreate(Endpoint, UriKind.Absolute, out _))
            {
                throw new InvalidOperationException("AI:Endpoint must be an absolute URI.");
            }
            ArgumentException.ThrowIfNullOrWhiteSpace(Model);
        }
    }
}
