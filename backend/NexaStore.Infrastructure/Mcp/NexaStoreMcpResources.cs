using ModelContextProtocol.Server;
using System.Text.Json;

namespace NexaStore.Infrastructure.Mcp
{
    [McpServerResourceType]
    public sealed class NexaStoreMcpResources
    {
        [McpServerResource(
            UriTemplate = "nexastore://capabilities",
            Name = "nexastore-capabilities",
            Title = "NexaStore MCP Capabilities",
            MimeType = "application/JSON")]

        public string GetCapabilities() => JsonSerializer.Serialize(new
        {
            service = "NexaStore",
            version = "1.0",
            transport = "Streamable HTTP",
            access = "read-only",
            domains = new[] { "catalogue", "product-details", "inventory", "categories", "knowledge-base"},
            grounding = new
            {
                retrieval = "hybrid",
                citations = true,
                insufficientEvidenceRefusal = true,
            },
            boundaries = new[] {"no carts", "no orders", "no customer data", "no write operations"}
        });
    }
}
