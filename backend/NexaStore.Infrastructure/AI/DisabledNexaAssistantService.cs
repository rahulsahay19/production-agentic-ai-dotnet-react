using NexaStore.Application.AI;
using System.Runtime.CompilerServices;

namespace NexaStore.Infrastructure.AI
{
    public sealed class DisabledNexaAssistantService : INexaAssistantService
    {
        public async IAsyncEnumerable<AssistantStreamUpdate> StreamResponseAsync(
            IReadOnlyList<AssistantMessageDto> messages, 
            string responseStyles = AssistantResponseStyles.Balanced,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            yield return AssistantStreamUpdate.Delta("Nexa AI is disabled in this environment");
        }
    }
}
