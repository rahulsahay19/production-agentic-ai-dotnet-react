using Microsoft.Extensions.Logging;

namespace NexaStore.Infrastructure.AI
{
    public sealed class LoggingAgentRunObserver(ILogger<LoggingAgentRunObserver> logger) : IAgentRunObserver
    {
        public ValueTask OnEventAsync(AgentRunEvent runEvent, CancellationToken cancellationToken = default)
        {
            logger.LogDebug("Agent harness lifecycle event {EventType} for run {AgentRunid}, agent {AgentName}: {@EventData}",
                runEvent.Type,
                runEvent.RunId,
                runEvent.AgentName,
                runEvent.Data);
            return ValueTask.CompletedTask;
        }
    }
}
