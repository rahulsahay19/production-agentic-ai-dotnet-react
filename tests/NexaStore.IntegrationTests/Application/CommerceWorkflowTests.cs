using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexaStore.Application;
using NexaStore.Application.Abstractions;
using NexaStore.Application.Cart;
using NexaStore.Application.Catalog;
using NexaStore.Application.Orders;
using NexaStore.Domain.Orders;
using NexaStore.Infrastructure;
using NexaStore.Infrastructure.Persistence;
using NexaStore.Infrastructure.Persistence.Seeding;

namespace NexaStore.IntegrationTests.Application;

public sealed class CommerceWorkflowTests
{
    [Fact]
    public async Task CustomerCanSearchAddOrderAndCancelWithInventoryRestored()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddInfrastructure(options => options.UseSqlite(connection));
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NexaStoreDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        await scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>().SeedAsync();

        var products = scope.ServiceProvider.GetRequiredService<IProductService>();
        var cart = scope.ServiceProvider.GetRequiredService<ICartService>();
        var orders = scope.ServiceProvider.GetRequiredService<IOrderService>();
        var inventory = scope.ServiceProvider.GetRequiredService<IInventoryService>();
        var customerId = await dbContext.Customers.Select(customer => customer.Id).SingleAsync();
        var search = await products.GetProductsAsync(
            new ProductSearchRequest(Keyword: "AI Studio 16"),
            CancellationToken.None);
        var product = Assert.Single(search.Items);

        var updatedCart = await cart.AddItemAsync(
            customerId,
            new AddCartItemRequest(product.Id, 1),
            CancellationToken.None);
        Assert.Equal(239990m, updatedCart.Subtotal);

        var address = new AddressDto(
            "Aarav Sharma", "101 Developer Avenue", "Tech Park", "Bengaluru",
            "Karnataka", "560001", "IN", "+91-9000000000");
        var order = await orders.CreateOrderAsync(
            new CreateOrderRequest(customerId, address, address),
            CancellationToken.None);
        Assert.Equal(0m, order.ShippingAmount);
        Assert.Equal(43198.20m, order.TaxAmount);
        Assert.Equal(283188.20m, order.TotalAmount);
        Assert.Equal(11, (await inventory.GetInventoryAsync(product.Id, CancellationToken.None)).QuantityAvailable);

        var cancelled = await orders.CancelOrderAsync(order.Id, CancellationToken.None);
        Assert.Equal(OrderStatus.Cancelled, cancelled.OrderStatus);
        Assert.Equal(12, (await inventory.GetInventoryAsync(product.Id, CancellationToken.None)).QuantityAvailable);
    }
}
