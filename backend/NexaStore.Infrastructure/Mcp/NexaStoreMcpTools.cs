using ModelContextProtocol.Server;
using NexaStore.Infrastructure.AI;
using System.ComponentModel;

namespace NexaStore.Infrastructure.Mcp
{
    [McpServerToolType]
    public sealed class NexaStoreMcpTools(NexaStoreAiTools tools)
    {
        [McpServerTool(Name ="search_products", Title = "Search NexaStore products", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false,
            UseStructuredContent = true)]
        [Description("Search the live NexaStore product catalogue using bounded purchasing requirements.")]
        public Task<ProductSearchToolResult> SearchProductsAsync(
        [Description("Words from the desired product name, description, SKU, or use case.")] string? keyword = null,
        [Description("Category name or slug.")] string? category = null,
        [Description("Brand name.")] string? brand = null,
        [Description("Maximum price in the listed currency.")] decimal? maximumPrice = null,
        [Description("Minimum system RAM in GB.")] int? minimumRamGb = null,
        [Description("Minimum GPU VRAM in GB.")] int? minimumVramGb = null,
        [Description("Require AI-ready products.")] bool? aiReady = null,
        [Description("Require local-LLM-ready products.")] bool? localLlmReady = null,
        CancellationToken cancellationToken = default) =>
        tools.SearchProductsAsync(keyword, category, brand, maximumPrice, minimumRamGb, minimumVramGb, aiReady, localLlmReady, cancellationToken);

        [McpServerTool(Name = "get_product_details", Title = "Get product details", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
        [Description("Get current specifications, capabilities, price, and stock for one product ID.")]
        public Task<ProductDetailsLookupToolResult> GetProductDetailsAsync(Guid productId, CancellationToken cancellationToken = default) =>
        tools.GetProductDetailsAsync(productId, cancellationToken);

        [McpServerTool(Name = "list_product_categories", Title = "List product categories", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
        [Description("List the current NexaStore product categories and slugs.")]
        public Task<IReadOnlyCollection<CategoryToolResult>> ListCategoriesAsync(CancellationToken cancellationToken = default) =>
            tools.ListCategoriesAsync(cancellationToken);

        [McpServerTool(Name = "check_inventory", Title = "Check inventory", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
        [Description("Check live inventory availability for a product and a requested quantity from 1 through 20.")]
        public Task<InventoryToolResult> CheckInventoryAsync(Guid productId, int quantity, CancellationToken cancellationToken = default) =>
            tools.CheckInventoryAsync(productId, quantity, cancellationToken);

        [McpServerTool(Name = "search_knowledge_base", Title = "Search NexaStore knowledge", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
        [Description("Search indexed NexaStore policies, support guides, buying guides, product manuals, programmes, and catalogues with grounded citations.")]
        public Task<KnowledgeSearchToolResult> SearchKnowledgeAsync(
            [Description("Focused question or search query.")] string query,
            [Description("Number of passages to return, from 1 through 8.")] int limit = 5,
            CancellationToken cancellationToken = default) =>
            tools.SearchKnowledgeAsync(query, limit, cancellationToken);
    }
}
