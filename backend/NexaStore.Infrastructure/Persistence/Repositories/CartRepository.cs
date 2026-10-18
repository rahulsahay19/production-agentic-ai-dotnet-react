using Microsoft.EntityFrameworkCore;
using NexaStore.Application.Abstractions;
using NexaStore.Domain.Cart;

namespace NexaStore.Infrastructure.Persistence.Repositories;

internal sealed class CartRepository(NexaStoreDbContext dbContext) : ICartRepository
{
    public Task<Cart?> GetByCustomerIdAsync(Guid customerId, bool trackChanges, CancellationToken cancellationToken)
    {
        var query = dbContext.Carts.Include(cart => cart.Items).AsQueryable();
        if (!trackChanges) query = query.AsNoTracking();
        return query.SingleOrDefaultAsync(cart => cart.CustomerId == customerId, cancellationToken);
    }

    public void Add(Cart cart) => dbContext.Carts.Add(cart);
}
