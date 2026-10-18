using NexaStore.Application.Cart;
using NexaStore.Application.Catalog;
using NexaStore.Application.Customers;
using NexaStore.Application.Inventory;
using NexaStore.Application.Orders;

namespace NexaStore.Application.Abstractions;

public interface IProductService
{
    Task<ProductSearchResult> GetProductsAsync(ProductSearchRequest request, CancellationToken cancellationToken);
    Task<ProductDetailsDto> GetProductByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken);
}

public interface ICustomerService
{
    Task<CustomerDto> GetCustomerAsync(Guid id, CancellationToken cancellationToken);
}

public interface ICartService
{
    Task<CartDto> GetCartAsync(Guid customerId, CancellationToken cancellationToken);
    Task<CartDto> AddItemAsync(Guid customerId, AddCartItemRequest request, CancellationToken cancellationToken);
    Task<CartDto> UpdateItemAsync(Guid customerId, Guid productId, UpdateCartItemRequest request, CancellationToken cancellationToken);
    Task RemoveItemAsync(Guid customerId, Guid productId, CancellationToken cancellationToken);
    Task ClearAsync(Guid customerId, CancellationToken cancellationToken);
}

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken);
    Task<OrderDto> GetOrderAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<OrderDto>> GetOrdersByCustomerAsync(Guid customerId, CancellationToken cancellationToken);
    Task<OrderDto> CancelOrderAsync(Guid id, CancellationToken cancellationToken);
}

public interface IInventoryService
{
    Task<InventoryDto> GetInventoryAsync(Guid productId, CancellationToken cancellationToken);
    Task<InventoryAvailabilityDto> CheckAvailabilityAsync(Guid productId, int quantity, CancellationToken cancellationToken);
}
