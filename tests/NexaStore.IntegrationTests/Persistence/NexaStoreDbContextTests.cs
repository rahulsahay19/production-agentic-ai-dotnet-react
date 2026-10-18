using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NexaStore.Infrastructure.Persistence;
using NexaStore.Infrastructure.Persistence.Seeding;

namespace NexaStore.IntegrationTests.Persistence;

public sealed class NexaStoreDbContextTests
{
    [Fact]
    public async Task SeedAsync_CreatesExpectedCatalogAndInventoryData()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<NexaStoreDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dbContext = new NexaStoreDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var seeder = new DevelopmentDataSeeder(dbContext);
        await seeder.SeedAsync();
        await seeder.SeedAsync();

        Assert.Equal(11, await dbContext.Categories.CountAsync());
        Assert.Equal(28, await dbContext.Products.CountAsync());
        Assert.Equal(28, await dbContext.Inventory.CountAsync());
        var customer = Assert.Single(await dbContext.Customers.ToArrayAsync());
        Assert.Equal(DevelopmentDataSeeder.DemoCustomerId, customer.Id);
        Assert.Equal(28, await dbContext.Products.Select(product => product.Sku).Distinct().CountAsync());

        var aiStudio = await dbContext.Products
            .Include(product => product.Images)
            .Include(product => product.Tags)
            .SingleAsync(product => product.Sku == "NB-AIS16-2026");
        var inventory = await dbContext.Inventory.SingleAsync(item => item.ProductId == aiStudio.Id);

        Assert.Equal(239990m, aiStudio.Price);
        Assert.Equal("Nexa", aiStudio.Brand);
        Assert.True(aiStudio.IsFeatured);
        Assert.Equal(12, aiStudio.CpuCores);
        Assert.Equal(99m, aiStudio.BatteryWh);
        Assert.Equal(2.35m, aiStudio.WeightKg);
        Assert.Equal(5, aiStudio.Tags.Count);
        Assert.Equal(2, aiStudio.Images.Count);
        Assert.Single(aiStudio.Images, image => image.IsPrimary);
        Assert.Equal(12, inventory.QuantityOnHand);
    }

    [Fact]
    public void Model_ContainsExpectedPersistenceEntities()
    {
        var options = new DbContextOptionsBuilder<NexaStoreDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        using var dbContext = new NexaStoreDbContext(options);
        string[] expectedEntities =
        [
            "Cart", "CartItem", "Category", "Customer", "InventoryItem", "Order", "OrderItem",
            "Product", "ProductImage", "ProductSpecification", "ProductTag"
        ];

        var mappedNames = dbContext.Model.GetEntityTypes()
            .Where(entityType => !entityType.IsOwned())
            .Select(entityType => entityType.ClrType.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.All(expectedEntities, name => Assert.Contains(name, mappedNames));
    }
}
