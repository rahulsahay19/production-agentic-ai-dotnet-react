namespace NexaStore.Infrastructure.Mcp
{
    public sealed class McpOptions
    {
        public const string SectionName = "Mcp";
        public bool Enabled { get; init; } = true;
        public bool UseForAssistant { get; init; } = true;
        public string ServerUrl { get; init; } = "http://localhost:5175/mcp";
        public int ConnectionTimeoutSeconds { get; init; } = 15;
        public string[] AllowedTools { get; init; } = [];

       public void Validate()
        {
            if(!Enabled) return;
            if(!Uri.TryCreate(ServerUrl, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
            {
                throw new InvalidOperationException("Mcp:ServerUrl must be an absolute HTTP or HTTPS URL.");
            }
            if(ConnectionTimeoutSeconds is < 1 or > 120)
            {
                throw new InvalidOperationException("Mcp:ConnectionTimeoutSeconds must be between 1 and 120");
            }
            if(AllowedTools.Length == 0 || AllowedTools.Any(string.IsNullOrWhiteSpace))
            {
                throw new InvalidOperationException("Mcp:AllowedTools must contain at least one valid tool name.");
            }
            if(AllowedTools.Distinct(StringComparer.Ordinal).Count()!= AllowedTools.Length)
            {
                throw new InvalidOperationException("Mcp:AllowedTools cannot contain duplicate tool name");
            }    
        }
    }
}
