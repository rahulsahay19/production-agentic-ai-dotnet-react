using NexaStore.Application.AI;

namespace NexaStore.Infrastructure.AI
{
    public interface IJevDecisionService
    {
        JevDecisionDto Decide(IReadOnlyList<AssistantMessageDto> messages);
    }

    public sealed class JevDecisionService(JevOptions options) : IJevDecisionService
    {
        private static readonly string[] RestrictedPhrases =
         [
           "add to cart", "place the order", "place an order", "buy it for me",
            "purchase it for me", "cancel my order", "cancel the order",
            "issue a refund", "process a refund", "change my account", "delete my account"
        ];

        private static readonly string[] KnowledgeTerms =
        [
            "policy", "return window", "warranty", "buying guide", "guide",
            "manual", "support document", "customer program", "nexaplus", "nexarewards",
            "cite", "source", "document"
        ];

        private static readonly string[] CommerceTerms =
        [
            "product", "laptop", "gpu", "workstation", "monitor", "price", "cost",
            "stock", "inventory", "available", "specification", "vram", "ram"
        ];

        private static readonly string[] InventoryTerms =
        [
            "stock", "inventory", "available", "availability", "units", "quantity"
        ];
        public JevDecisionDto Decide(IReadOnlyList<AssistantMessageDto> messages)
        {
            var request = messages.LastOrDefault(message =>
                string.Equals(message.Role, "user", StringComparison.OrdinalIgnoreCase))?.Content ?? string.Empty;
            var normalized = request.Trim().ToLowerInvariant();

            if (!options.Enabled)
            {
                return new JevDecisionDto(
                    JevRoutes.General, JevRisk.Low, JevEvidence.None, true,
                    "jev-disabled", []);
            }

            if (options.BlockUnsupportedMutations && IsRestrictedOperation(normalized))
            {
                return new JevDecisionDto(
                    JevRoutes.Restricted, JevRisk.High, JevEvidence.None, false,
                    "unsupported-write-operation", []);
            }

            var needsKnowledge = ContainsAny(normalized, KnowledgeTerms);
            var needsCommerce = ContainsAny(normalized, CommerceTerms);
            var needsInventory = ContainsAny(normalized, InventoryTerms);

            if (!needsKnowledge && IsGeneralConceptQuestion(normalized))
            {
                return new JevDecisionDto(
                    JevRoutes.General, JevRisk.Low, JevEvidence.None, true,
                    "general-technical-guidance", []);
            }

            if (needsKnowledge && needsCommerce)
            {
                var tools = new List<string> { "search_knowledge_base", "search_products" };
                if (needsInventory) tools.Add("check_inventory");
                return new JevDecisionDto(
                    JevRoutes.Mixed, JevRisk.Medium, JevEvidence.Combined, true,
                    "document-and-live-commerce-claims", tools);
            }

            if (needsKnowledge)
            {
                return new JevDecisionDto(
                    JevRoutes.Knowledge, JevRisk.Medium, JevEvidence.KnowledgeBase, true,
                    "document-grounded-claim", ["search_knowledge_base"]);
            }

            if (needsCommerce)
            {
                var tools = new List<string> { "search_products" };
                if (needsInventory) tools.Add("check_inventory");
                return new JevDecisionDto(
                    JevRoutes.Commerce, JevRisk.Medium, JevEvidence.LiveCommerce, true,
                    "live-commerce-claim", tools);
            }

            return new JevDecisionDto(
                JevRoutes.General, JevRisk.Low, JevEvidence.None, true,
                "general-technical-guidance", []);
        }

        private static bool ContainsAny(string value, IEnumerable<string> terms)
            => terms.Any(term => value.Contains(term, StringComparison.Ordinal));

        private static bool IsRestrictedOperation(string value) =>
            ContainsAny(value, RestrictedPhrases) || (value.StartsWith("add ", StringComparison.Ordinal)
            && value.Contains("cart", StringComparison.Ordinal));


        private static bool IsGeneralConceptQuestion(string value) =>
            (value.StartsWith("what is ", StringComparison.Ordinal)
             || value.StartsWith("what are ", StringComparison.Ordinal)
             || value.Contains("difference between", StringComparison.Ordinal)
             || value.StartsWith("explain ", StringComparison.Ordinal))
            && !value.Contains("nexastore", StringComparison.Ordinal)
            && !value.Contains("current", StringComparison.Ordinal)
            && !value.Contains("available", StringComparison.Ordinal)
            && !value.Contains("product", StringComparison.Ordinal)
            && !value.Contains("laptop", StringComparison.Ordinal)
            && !value.Contains("workstation", StringComparison.Ordinal);

        public static class JevRoutes
        {
            public const string General = "general";
            public const string Commerce = "commerce";
            public const string Knowledge = "knowledge";
            public const string Mixed = "mixed";
            public const string Restricted = "restricted";
            public static readonly IReadOnlyList<string> All = [General, Commerce, Knowledge, Mixed, Restricted];
        }
        public static class JevRisk
        {
            public const string Low = "low";
            public const string Medium = "medium";
            public const string High = "high";
        }
        public static class JevEvidence
        {
            public const string None = "none";
            public const string LiveCommerce = "live-commerce";
            public const string KnowledgeBase = "knowledge-base";
            public const string Combined = "knowledge-and-live-commerce";
            public static readonly IReadOnlyList<string> All = [None, LiveCommerce, KnowledgeBase, Combined];
        }
    }
}
