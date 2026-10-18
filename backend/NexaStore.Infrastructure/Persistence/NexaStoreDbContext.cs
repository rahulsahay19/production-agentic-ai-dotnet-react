using Microsoft.EntityFrameworkCore;
using NexaStore.Application.Abstractions;
using NexaStore.Domain.Cart;
using NexaStore.Domain.Catalog;
using NexaStore.Domain.Customers;
using NexaStore.Domain.Inventory;
using NexaStore.Domain.Orders;

namespace NexaStore.Infrastructure.Persistence;

public sealed class NexaStoreDbContext(DbContextOptions<NexaStoreDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<InventoryItem> Inventory => Set<InventoryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NexaStoreDbContext).Assembly);
        if (Database.IsNpgsql())
        {
            modelBuilder.Entity<InventoryItem>().Property(item => item.Version).IsRowVersion();
            modelBuilder.Entity<Order>().Property(order => order.Version).IsRowVersion();
        }
        else
        {
            modelBuilder.Entity<InventoryItem>().Property(item => item.Version).IsConcurrencyToken().ValueGeneratedNever();
            modelBuilder.Entity<Order>().Property(order => order.Version).IsConcurrencyToken().ValueGeneratedNever();
        }
        base.OnModelCreating(modelBuilder);
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        if (Database.CurrentTransaction is not null)
        {
            return await operation(cancellationToken);
        }

        var strategy = Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        });
    }
}
