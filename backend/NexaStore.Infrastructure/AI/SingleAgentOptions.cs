namespace NexaStore.Infrastructure.AI
{
    public sealed class SingleAgentOptions
    {
        public static readonly string[] DefaultAllowedTools =
           [
                "search_products",
                "get_product_details",
                "list_product_categories",
                "check_inventory",
                "search_knowledge_base"
           ];
        public const string SectionName = "Agent";
        public string Name { get; init; } = "NexaStore Support & Shopping Agent";
        public int MaximumToolIterations { get; init; } = 5;

        public string[] AllowedTools { get; init; } = [];

        public IReadOnlyList<string> EffectiveAllowedTools =>
            AllowedTools.Length == 0 ? DefaultAllowedTools : AllowedTools;
             

        public void Validate()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(Name);
            if(MaximumToolIterations is < 1 or > 10)
            {
                throw new InvalidOperationException("Agent:MaximumToolIterations must be between 1 and 10");
            }
        }
    }
}
