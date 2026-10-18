using Microsoft.EntityFrameworkCore;
using NexaStore.Application.Abstractions;
using NexaStore.Application.Catalog;
using NexaStore.Domain.Catalog;

namespace NexaStore.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository(NexaStoreDbContext dbContext) : IProductRepository
{
    public async Task<(IReadOnlyCollection<Product> Items, int TotalCount)> SearchAsync(
        ProductSearchRequest request,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Products.AsNoTracking().Where(product => product.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim().ToLower();
            query = query.Where(product =>
                product.Name.ToLower().Contains(keyword) ||
                product.Sku.ToLower().Contains(keyword) ||
                product.Brand.ToLower().Contains(keyword) ||
                product.ShortDescription.ToLower().Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var category = request.Category.Trim().ToLower();
            query = query.Where(product => dbContext.Categories.Any(candidate =>
                candidate.Id == product.CategoryId &&
                (candidate.Slug == category || candidate.Name.ToLower() == category)));
        }

        if (!string.IsNullOrWhiteSpace(request.Brand))
        {
            var brand = request.Brand.Trim().ToLower();
            query = query.Where(product => product.Brand.ToLower() == brand);
        }

        if (request.MinimumPrice is not null) query = query.Where(product => product.Price >= request.MinimumPrice);
        if (request.MaximumPrice is not null) query = query.Where(product => product.Price <= request.MaximumPrice);
        if (request.MinimumRamGb is not null) query = query.Where(product => product.RamGb >= request.MinimumRamGb);
        if (request.MinimumVramGb is not null) query = query.Where(product => product.VramGb >= request.MinimumVramGb);
        if (request.AiReady is not null) query = query.Where(product => product.IsAiReady == request.AiReady);
        if (request.LocalLlmReady is not null) query = query.Where(product => product.IsLocalLlmReady == request.LocalLlmReady);
        if (request.DeveloperReady is not null) query = query.Where(product => product.IsDeveloperReady == request.DeveloperReady);
        if (request.CreatorReady is not null) query = query.Where(product => product.IsCreatorReady == request.CreatorReady);
        if (request.GamingReady is not null) query = query.Where(product => product.IsGamingReady == request.GamingReady);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Include(product => product.Images)
            .OrderByDescending(product => product.IsFeatured)
            .ThenBy(product => product.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToArrayAsync(cancellationToken);
        return (items, totalCount);
    }

    public Task<Product?> GetByIdAsync(Guid id, bool trackChanges, CancellationToken cancellationToken)
    {
        var query = dbContext.Products
            .Include(product => product.Images)
            .Include(product => product.Tags)
            .Include(product => product.Specifications)
            .AsQueryable();
        if (!trackChanges) query = query.AsNoTracking();
        return query.SingleOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Category>> GetCategoriesAsync(CancellationToken cancellationToken) =>
        await dbContext.Categories.AsNoTracking().OrderBy(category => category.Name).ToArrayAsync(cancellationToken);

    public Task<string?> GetCategoryNameAsync(Guid categoryId, CancellationToken cancellationToken) =>
        dbContext.Categories.AsNoTracking()
            .Where(category => category.Id == categoryId)
            .Select(category => category.Name)
            .SingleOrDefaultAsync(cancellationToken);
}
