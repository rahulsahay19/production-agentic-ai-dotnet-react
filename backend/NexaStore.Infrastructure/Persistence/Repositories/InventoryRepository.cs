using Microsoft.EntityFrameworkCore;
using NexaStore.Application.Abstractions;
using NexaStore.Domain.Inventory;

namespace NexaStore.Infrastructure.Persistence.Repositories;

internal sealed class InventoryRepository(NexaStoreDbContext dbContext) : IInventoryRepository
{
    public Task<InventoryItem?> GetByProductIdAsync(
        Guid productId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Inventory.AsQueryable();
        if (!trackChanges) query = query.AsNoTracking();
        return query.SingleOrDefaultAsync(item => item.ProductId == productId, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, int>> GetAvailableQuantitiesAsync(
        IEnumerable<Guid> productIds,
        CancellationToken cancellationToken)
    {
        var ids = productIds.Distinct().ToArray();
        return await dbContext.Inventory.AsNoTracking()
            .Where(item => ids.Contains(item.ProductId))
            .ToDictionaryAsync(
                item => item.ProductId,
                item => item.QuantityOnHand - item.QuantityReserved,
                cancellationToken);
    }
}
