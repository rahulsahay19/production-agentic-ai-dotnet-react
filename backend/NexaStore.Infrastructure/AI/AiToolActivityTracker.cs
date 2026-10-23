using NexaStore.Application.AI;
using System.Collections.Concurrent;

namespace NexaStore.Infrastructure.AI
{
    public sealed class AiToolActivityTracker
    {
        private readonly ConcurrentQueue<AssistantStreamUpdate> _activities = new();

        public void Record(string toolName, string label) =>
            _activities.Enqueue(AssistantStreamUpdate.Tool(toolName, label));

        public IEnumerable<AssistantStreamUpdate> Drain()
        {
            while (_activities.TryDequeue(out var activity))
            {
                yield return activity;
            } 
        }
    }
}
