using NexaStore.Application.Abstractions;
using NexaStore.Application.Common;
using NexaStore.Domain.Catalog;
using Microsoft.Extensions.Logging;

namespace NexaStore.Application.Catalog;

public sealed class ProductService(
    IProductRepository products,
    IInventoryRepository inventory,
    ILogger<ProductService> logger) : IProductService
{
    public async Task<ProductSearchResult> GetProductsAsync(
        ProductSearchRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request);
        var (items, totalCount) = await products.SearchAsync(request, cancellationToken);
        var categories = (await products.GetCategoriesAsync(cancellationToken)).ToDictionary(category => category.Id);
        var stock = await inventory.GetAvailableQuantitiesAsync(items.Select(product => product.Id), cancellationToken);
        var results = items.Select(product => ToSummary(
            product,
            categories[product.CategoryId].Name,
            stock.GetValueOrDefault(product.Id))).ToArray();

        logger.LogInformation(
            "Product search returned {ResultCount} of {TotalCount} products for page {Page}",
            results.Length,
            totalCount,
            request.Page);

        return new ProductSearchResult(results, totalCount, request.Page, request.PageSize);
    }

    public async Task<ProductDetailsDto> GetProductByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(id, false, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);
        var categoryName = await products.GetCategoryNameAsync(product.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), product.CategoryId);
        var stock = await inventory.GetAvailableQuantitiesAsync([product.Id], cancellationToken);

        return ToDetails(product, categoryName, stock.GetValueOrDefault(product.Id));
    }

    public async Task<IReadOnlyCollection<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken) =>
        (await products.GetCategoriesAsync(cancellationToken))
            .Select(category => new CategoryDto(category.Id, category.Name, category.Slug, category.Description))
            .ToArray();

    private static ProductSummaryDto ToSummary(Product product, string category, int stockQuantity) => new(
        product.Id,
        product.Sku,
        product.Name,
        product.Slug,
        product.Brand,
        category,
        product.ShortDescription,
        product.Price,
        product.Currency,
        stockQuantity,
        product.IsFeatured,
        product.IsAiReady,
        product.IsLocalLlmReady,
        product.IsDeveloperReady,
        product.IsCreatorReady,
        product.IsGamingReady,
        product.Images.OrderByDescending(image => image.IsPrimary).ThenBy(image => image.DisplayOrder).FirstOrDefault()?.Url);

    private static ProductDetailsDto ToDetails(Product product, string category, int stockQuantity) => new(
        product.Id,
        product.Sku,
        product.Name,
        product.Slug,
        product.Brand,
        product.CategoryId,
        category,
        product.ShortDescription,
        product.Description,
        product.Price,
        product.Currency,
        stockQuantity,
        product.IsActive,
        product.IsFeatured,
        new ProductTechnicalSpecificationsDto(
            product.Cpu, product.CpuCores, product.RamGb, product.MaxRamGb, product.Gpu, product.VramGb,
            product.Npu, product.NpuTops, product.StorageGb, product.StorageType, product.Display,
            product.BatteryWh, product.WeightKg),
        new ProductCapabilitiesDto(
            product.IsAiReady, product.IsLocalLlmReady, product.IsDeveloperReady,
            product.IsCreatorReady, product.IsGamingReady, product.IsDockerReady),
        product.Tags.OrderBy(tag => tag.Name).Select(tag => tag.Name).ToArray(),
        product.Images.OrderBy(image => image.DisplayOrder)
            .Select(image => new ProductImageDto(image.Url, image.AltText, image.IsPrimary, image.DisplayOrder)).ToArray(),
        product.Specifications.OrderBy(specification => specification.DisplayOrder)
            .Select(specification => new ProductSpecificationDto(
                specification.Group, specification.Name, specification.Value, specification.DisplayOrder)).ToArray());

    private static void Validate(ProductSearchRequest request)
    {
        if (request.Page < 1)
        {
            throw new ApplicationValidationException("Page must be at least 1.");
        }

        if (request.PageSize is < 1 or > 100)
        {
            throw new ApplicationValidationException("Page size must be between 1 and 100.");
        }

        if (request.MinimumPrice < 0 || request.MaximumPrice < 0 || request.MinimumPrice > request.MaximumPrice)
        {
            throw new ApplicationValidationException("The price range is invalid.");
        }
    }
}
