using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using NexaStore.Infrastructure.Mcp;

namespace NexaStore.Infrastructure.AI;

public interface IAssistantToolProvider
{
    ValueTask<IList<AITool>> GetToolsAsync(CancellationToken cancellationToken = default);
}

public sealed class LocalAssistantToolProvider(NexaStoreAiTools tools) : IAssistantToolProvider
{
    public ValueTask<IList<AITool>> GetToolsAsync(CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(tools.CreateAll());
}

public sealed class McpAssistantToolProvider(
    McpOptions options,
    ILoggerFactory loggerFactory,
    ILogger<McpAssistantToolProvider> logger) : IAssistantToolProvider, IAsyncDisposable
{
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private McpClient? _client;
    private IList<AITool>? _tools;

    public async ValueTask<IList<AITool>> GetToolsAsync(CancellationToken cancellationToken = default)
    {
        if (_tools is not null) return _tools;

        //Acquire the Initialization Lock
        await _initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_tools is not null) return _tools;

            //MCP Http Transport
            var transport = new HttpClientTransport(
                new HttpClientTransportOptions
                {
                    Endpoint = new Uri(options.ServerUrl),
                    Name = "NexaStore MCP",
                    ConnectionTimeout = TimeSpan.FromSeconds(options.ConnectionTimeoutSeconds)
                },
                loggerFactory);
            // Creating the MCP Client
            _client = await McpClient.CreateAsync(transport, loggerFactory: loggerFactory, cancellationToken: cancellationToken);
            // MCP Mode:- Discover tools dynamically
            var discovered = await _client.ListToolsAsync(cancellationToken: cancellationToken);
            var allowed = options.AllowedTools.ToHashSet(StringComparer.Ordinal);
            _tools = discovered
                .Where(tool => allowed.Contains(tool.Name))
                .Cast<AITool>()
                .ToArray();

            var missing = allowed.Except(_tools.Select(tool => tool.Name), StringComparer.Ordinal).ToArray();
            if (missing.Length > 0)
            {
                await _client.DisposeAsync();
                _client = null;
                _tools = null;
                throw new InvalidOperationException($"MCP server did not expose required tools: {string.Join(", ", missing)}");
            }

            logger.LogInformation(
                "Discovered {Count} allowlisted tools from MCP server {ServerUrl}: {ToolNames}",
                _tools.Count,
                options.ServerUrl,
                string.Join(", ", _tools.Select(tool => tool.Name)));
            return _tools;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_client is not null) await _client.DisposeAsync();
        _initializationLock.Dispose();
    }
}
