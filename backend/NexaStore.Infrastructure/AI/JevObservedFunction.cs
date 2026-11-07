using Microsoft.Extensions.AI;
using System.Reflection;
using System.Text.Json;

namespace NexaStore.Infrastructure.AI
{
    internal sealed class JevObservedFunction(AIFunction inner, JevRunContext context) : AIFunction
    {
        public override string Name => inner.Name;
        public override string Description => inner.Description;
        public override IReadOnlyDictionary<string, object?> AdditionalProperties => inner.AdditionalProperties;
        public override JsonElement JsonSchema => inner.JsonSchema;
        public override JsonElement? ReturnJsonSchema => inner.ReturnJsonSchema;
        public override JsonSerializerOptions JsonSerializerOptions => inner.JsonSerializerOptions;
        public override MethodInfo? UnderlyingMethod => inner.UnderlyingMethod;
        protected override async ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
        {
            var result = await inner.InvokeAsync(arguments, cancellationToken).ConfigureAwait(false);
            context.RecordTool(inner.Name);
            return result;
        }
    }
}
