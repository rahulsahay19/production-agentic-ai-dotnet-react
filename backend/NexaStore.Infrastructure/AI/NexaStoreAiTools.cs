using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using NexaStore.Application.Abstractions;
using NexaStore.Application.Catalog;
using NexaStore.Application.Common;
using NexaStore.Application.Knowledge;
using System.ComponentModel;

namespace NexaStore.Infrastructure.AI
{
    public sealed class NexaStoreAiTools(
    IProductService products,
    IInventoryService inventory,
    IKnowledgeService knowledge,
    AiToolActivityTracker activityTracker,
    ILogger<NexaStoreAiTools> logger)
    {
        public IList<AITool> CreateAll() =>
        [
            AIFunctionFactory.Create(
            (Func<string?, string?, string?, decimal?, int?, int?, bool?, bool?, CancellationToken,
                Task<ProductSearchToolResult>>)SearchProductsAsync,
            name: "search_products",
            description: "Search the live NexaStore product catalog using optional purchasing requirements."),
        AIFunctionFactory.Create(
            (Func<Guid, CancellationToken, Task<ProductDetailsLookupToolResult>>)GetProductDetailsAsync,
            name: "get_product_details",
            description: "Get current specifications, capabilities, price, and stock for one product ID returned by search_products."),
        AIFunctionFactory.Create(
            (Func<CancellationToken, Task<IReadOnlyCollection<CategoryToolResult>>>)ListCategoriesAsync,
            name: "list_product_categories",
            description: "List the current NexaStore product categories and their slugs."),
        AIFunctionFactory.Create(
            (Func<Guid, int, CancellationToken, Task<InventoryToolResult>>)CheckInventoryAsync,
            name: "check_inventory",
            description: "Check whether a requested quantity of a product is currently available. Use a product ID returned by search_products."),
        AIFunctionFactory.Create(
            (Func<string, int, CancellationToken, Task<KnowledgeSearchToolResult>>)SearchKnowledgeAsync,
            name: "search_knowledge_base",
            description: "Search NexaStore PDFs for policies, support guidance, buying advice, customer programs, product manuals, and catalogues. Cite returned sources as [title, p. page].")
        ];

        [Description("Search the live NexaStore product catalog using optional purchasing requirements.")]
        public async Task<ProductSearchToolResult> SearchProductsAsync(
            [Description("Words from the desired product name, description, SKU, or use case.")] string? keyword = null,
            [Description("Category name or slug, when known.")] string? category = null,
            [Description("Brand name, when requested.")] string? brand = null,
            [Description("Maximum price in the product's listed currency, when the user gives a budget.")] decimal? maximumPrice = null,
            [Description("Minimum system RAM in GB, when required.")] int? minimumRamGb = null,
            [Description("Minimum dedicated GPU VRAM in GB, when required.")] int? minimumVramGb = null,
            [Description("True when the product must be suitable for AI workloads.")] bool? aiReady = null,
            [Description("True when the product must support local LLM workloads.")] bool? localLlmReady = null,
            CancellationToken cancellationToken = default)
        {
            if (maximumPrice < 0 || minimumRamGb < 0 || minimumVramGb < 0)
            {
                return new ProductSearchToolResult(
                    0,
                    [],
                    "Price, RAM, and VRAM filters cannot be negative.");
            }

            var result = await products.GetProductsAsync(new ProductSearchRequest(
                Keyword: Normalize(keyword),
                Category: Normalize(category),
                Brand: Normalize(brand),
                MaximumPrice: maximumPrice,
                MinimumRamGb: minimumRamGb,
                MinimumVramGb: minimumVramGb,
                AiReady: aiReady,
                LocalLlmReady: localLlmReady,
                PageSize: 5), cancellationToken);

            activityTracker.Record("search_products", "Searched the live product catalog");
            logger.LogInformation("AI tool search_products returned {Count} of {Total} products", result.Items.Count, result.TotalCount);

            return new ProductSearchToolResult(
                result.TotalCount,
                result.Items.Select(item => new ProductSummaryToolResult(
                    item.Id,
                    item.Sku,
                    item.Name,
                    item.Brand,
                    item.Category,
                    item.ShortDescription,
                    item.Price,
                    item.Currency,
                    item.StockQuantity,
                    item.IsAiReady,
                    item.IsLocalLlmReady)).ToArray(),
                null);
        }

        [Description("Get current product details for an ID returned by search_products.")]
        public async Task<ProductDetailsLookupToolResult> GetProductDetailsAsync(
            [Description("The product ID returned by search_products.")] Guid productId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var product = await products.GetProductByIdAsync(productId, cancellationToken);
                activityTracker.Record("get_product_details", "Read live product specifications");
                logger.LogInformation("AI tool get_product_details read product {ProductId}", productId);

                return new ProductDetailsLookupToolResult(true, new ProductDetailsToolResult(
                    product.Id,
                    product.Sku,
                    product.Name,
                    product.Brand,
                    product.Category,
                    product.Description,
                    product.Price,
                    product.Currency,
                    product.StockQuantity,
                    product.TechnicalSpecifications,
                    product.Capabilities,
                    product.Tags), null);
            }
            catch (NotFoundException)
            {
                return new ProductDetailsLookupToolResult(false, null, "Product was not found.");
            }
        }

        [Description("List the live NexaStore product categories.")]
        public async Task<IReadOnlyCollection<CategoryToolResult>> ListCategoriesAsync(
            CancellationToken cancellationToken = default)
        {
            var categories = await products.GetCategoriesAsync(cancellationToken);
            activityTracker.Record("list_product_categories", "Read live product categories");
            logger.LogInformation("AI tool list_product_categories returned {Count} categories", categories.Count);
            return categories.Select(category => new CategoryToolResult(
                category.Name,
                category.Slug,
                category.Description)).ToArray();
        }

        [Description("Check current inventory availability for a product and requested quantity.")]
        public async Task<InventoryToolResult> CheckInventoryAsync(
            [Description("The product ID returned by search_products.")] Guid productId,
            [Description("Desired quantity. Must be from 1 through 20.")] int quantity,
            CancellationToken cancellationToken = default)
        {
            if (quantity is < 1 or > 20)
            {
                return new InventoryToolResult(productId, quantity, false, 0, "Quantity must be between 1 and 20.");
            }

            try
            {
                var availability = await inventory.CheckAvailabilityAsync(productId, quantity, cancellationToken);
                activityTracker.Record("check_inventory", "Checked live inventory availability");
                logger.LogInformation(
                    "AI tool check_inventory checked product {ProductId} for quantity {Quantity}",
                    productId,
                    quantity);

                return new InventoryToolResult(
                    availability.ProductId,
                    availability.RequestedQuantity,
                    availability.IsAvailable,
                    availability.QuantityAvailable,
                    null);
            }
            catch (NotFoundException)
            {
                return new InventoryToolResult(productId, quantity, false, 0, "Product inventory was not found.");
            }
        }

        [Description("Search the indexed NexaStore PDF knowledge base and return source-grounded passages.")]
        public async Task<KnowledgeSearchToolResult> SearchKnowledgeAsync(
            [Description("A focused semantic search query describing the information needed.")] string query,
            [Description("Number of passages to return, from 1 through 8.")] int limit = 5,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new KnowledgeSearchToolResult([], "A search query is required.");
            }

            var results = await knowledge.SearchAsync(query, Math.Clamp(limit, 1, 8), cancellationToken);
            activityTracker.Record("search_knowledge_base", "Searched the indexed NexaStore documents");
            logger.LogInformation("AI tool search_knowledge_base returned {Count} passages", results.Count);
            return new KnowledgeSearchToolResult(
                results.Select(result => new KnowledgePassageToolResult(
                    result.DocumentTitle,
                    result.Category,
                    result.SourcePath,
                    result.PageNumber,
                    result.Content,
                    result.Relevance,
                    result.RetrievalMethod,
                    result.Citation)).ToArray(),
                results.Count == 0
                    ? "INSUFFICIENT_EVIDENCE: no passage met the configured grounding threshold. Say that the indexed documents do not establish the answer."
                    : null);
        }

        private static string? Normalize(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public sealed record ProductSearchToolResult(
        int TotalCount,
        IReadOnlyCollection<ProductSummaryToolResult> Items,
        string? Error);

    public sealed record ProductSummaryToolResult(
        Guid ProductId,
        string Sku,
        string Name,
        string Brand,
        string Category,
        string Description,
        decimal Price,
        string Currency,
        int QuantityAvailable,
        bool AiReady,
        bool LocalLlmReady);

    public sealed record ProductDetailsToolResult(
        Guid ProductId,
        string Sku,
        string Name,
        string Brand,
        string Category,
        string Description,
        decimal Price,
        string Currency,
        int QuantityAvailable,
        ProductTechnicalSpecificationsDto TechnicalSpecifications,
        ProductCapabilitiesDto Capabilities,
        IReadOnlyCollection<string> Tags);

    public sealed record ProductDetailsLookupToolResult(
        bool Found,
        ProductDetailsToolResult? Product,
        string? Error);

    public sealed record CategoryToolResult(string Name, string Slug, string? Description);

    public sealed record InventoryToolResult(
        Guid ProductId,
        int RequestedQuantity,
        bool IsAvailable,
        int QuantityAvailable,
        string? Error);

    public sealed record KnowledgeSearchToolResult(
        IReadOnlyCollection<KnowledgePassageToolResult> Passages,
        string? Error);

    public sealed record KnowledgePassageToolResult(
        string Title,
        string Category,
        string SourcePath,
        int PageNumber,
        string Content,
        double Relevance,
        string RetrievalMethod,
        string Citation);


}