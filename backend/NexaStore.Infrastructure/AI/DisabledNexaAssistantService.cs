using NexaStore.Application.AI;
using System.Runtime.CompilerServices;

namespace NexaStore.Infrastructure.AI
{
    public sealed class DisabledNexaAssistantService : INexaAssistantService
    {
        public async IAsyncEnumerable<string> StreamResponseAsync(
            IReadOnlyList<AssistantMessageDto> messages, 
            string responseStyles = AssistantResponseStyles.Balanced,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            yield return "Nexa AI is diabled in this enviornment.";
        }
    }
}
