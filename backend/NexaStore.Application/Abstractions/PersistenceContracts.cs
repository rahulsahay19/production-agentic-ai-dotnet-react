using NexaStore.Application.Catalog;
using NexaStore.Domain.Catalog;
using NexaStore.Domain.Customers;
using NexaStore.Domain.Inventory;
using NexaStore.Domain.Orders;

namespace NexaStore.Application.Abstractions;

public interface IProductRepository
{
    Task<(IReadOnlyCollection<Product> Items, int TotalCount)> SearchAsync(
        ProductSearchRequest request,
        CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(Guid id, bool trackChanges, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Category>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<string?> GetCategoryNameAsync(Guid categoryId, CancellationToken cancellationToken);
}

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}

public interface ICartRepository
{
    Task<NexaStore.Domain.Cart.Cart?> GetByCustomerIdAsync(Guid customerId, bool trackChanges, CancellationToken cancellationToken);
    void Add(NexaStore.Domain.Cart.Cart cart);
}

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Order>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken);
    void Add(Order order);
}

public interface IInventoryRepository
{
    Task<InventoryItem?> GetByProductIdAsync(Guid productId, bool trackChanges, CancellationToken cancellationToken);
    Task<IReadOnlyDictionary<Guid, int>> GetAvailableQuantitiesAsync(
        IEnumerable<Guid> productIds,
        CancellationToken cancellationToken);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken);
}
