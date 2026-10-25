using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NexaStore.Infrastructure.Persistence;

public sealed class NexaStoreDbContextFactory : IDesignTimeDbContextFactory<NexaStoreDbContext>
{
    public NexaStoreDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__NexaStore")
            ?? "Host=localhost;Database=nexastore";
        var options = new DbContextOptionsBuilder<NexaStoreDbContext>()
            .UseNpgsql(connectionString, npgsql => npgsql.UseVector())
            .Options;

        return new NexaStoreDbContext(options);
    }
}