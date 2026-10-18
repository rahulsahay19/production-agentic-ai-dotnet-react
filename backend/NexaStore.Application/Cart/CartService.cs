using NexaStore.Application.Abstractions;
using NexaStore.Application.Common;
using NexaStore.Domain.Catalog;
using NexaStore.Domain.Customers;
using NexaStore.Domain.Inventory;
using Microsoft.Extensions.Logging;

namespace NexaStore.Application.Cart;

public sealed class CartService(
    ICartRepository carts,
    IProductRepository products,
    ICustomerRepository customers,
    IInventoryRepository inventory,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    ILogger<CartService> logger) : ICartService
{
    public async Task<CartDto> GetCartAsync(Guid customerId, CancellationToken cancellationToken)
    {
        await EnsureCustomerExistsAsync(customerId, cancellationToken);
        var cart = await carts.GetByCustomerIdAsync(customerId, false, cancellationToken);
        return cart?.ToDto() ?? new CartDto(Guid.Empty, customerId, [], 0m, "INR");
    }

    public async Task<CartDto> AddItemAsync(
        Guid customerId,
        AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureCustomerExistsAsync(customerId, cancellationToken);
        var product = await GetActiveProductAsync(request.ProductId, cancellationToken);
        var inventoryItem = await GetInventoryAsync(request.ProductId, cancellationToken);
        var cart = await carts.GetByCustomerIdAsync(customerId, true, cancellationToken);
        if (cart is null)
        {
            cart = new NexaStore.Domain.Cart.Cart(customerId, timeProvider.GetUtcNow());
            carts.Add(cart);
        }

        cart.AddItem(
            product.Id,
            product.Sku,
            product.Name,
            product.Price,
            product.Currency,
            request.Quantity,
            inventoryItem.QuantityAvailable,
            timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Added product {ProductId} with quantity {Quantity} to cart for customer {CustomerId}",
            request.ProductId,
            request.Quantity,
            customerId);
        return cart.ToDto();
    }

    public async Task<CartDto> UpdateItemAsync(
        Guid customerId,
        Guid productId,
        UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var cart = await GetCartForUpdateAsync(customerId, cancellationToken);
        var inventoryItem = await GetInventoryAsync(productId, cancellationToken);
        cart.UpdateItem(productId, request.Quantity, inventoryItem.QuantityAvailable, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Updated product {ProductId} to quantity {Quantity} in cart for customer {CustomerId}",
            productId,
            request.Quantity,
            customerId);
        return cart.ToDto();
    }

    public async Task RemoveItemAsync(Guid customerId, Guid productId, CancellationToken cancellationToken)
    {
        var cart = await GetCartForUpdateAsync(customerId, cancellationToken);
        cart.RemoveItem(productId, timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Removed product {ProductId} from cart for customer {CustomerId}", productId, customerId);
    }

    public async Task ClearAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var cart = await GetCartForUpdateAsync(customerId, cancellationToken);
        cart.Clear(timeProvider.GetUtcNow());
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Cleared cart for customer {CustomerId}", customerId);
    }

    private async Task EnsureCustomerExistsAsync(Guid customerId, CancellationToken cancellationToken)
    {
        if (await customers.GetByIdAsync(customerId, cancellationToken) is null)
        {
            throw new NotFoundException(nameof(Customer), customerId);
        }
    }

    private async Task<Product> GetActiveProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(productId, false, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), productId);
        if (!product.IsActive)
        {
            throw new ApplicationValidationException("An inactive product cannot be added to the cart.");
        }

        return product;
    }

    private async Task<InventoryItem> GetInventoryAsync(Guid productId, CancellationToken cancellationToken) =>
        await inventory.GetByProductIdAsync(productId, false, cancellationToken)
        ?? throw new NotFoundException(nameof(InventoryItem), productId);

    private async Task<NexaStore.Domain.Cart.Cart> GetCartForUpdateAsync(Guid customerId, CancellationToken cancellationToken) =>
        await carts.GetByCustomerIdAsync(customerId, true, cancellationToken)
        ?? throw new NotFoundException(nameof(NexaStore.Domain.Cart.Cart), customerId);
}
