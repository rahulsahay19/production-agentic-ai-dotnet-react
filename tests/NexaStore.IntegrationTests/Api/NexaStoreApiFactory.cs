using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NexaStore.Infrastructure.Persistence;
using NexaStore.Infrastructure.Persistence.Seeding;
using NexaStore.Application.AI;
using System.Runtime.CompilerServices;

namespace NexaStore.IntegrationTests.Api;

public sealed class NexaStoreApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public Guid CustomerId { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("AI:Provider", "OpenAI");
        builder.UseSetting("AI:Providers:OpenAI:Endpoint", "https://api.openai.com/v1");
        builder.UseSetting("AI:Providers:OpenAI:Model", "gpt-5.6-terra");
        builder.UseSetting("AI:Providers:OpenAI:ApiKey", "test-key");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["ConnectionStrings:NexaStore"] = "Host=unused;Database=unused",
                ["Database:ApplyMigrationsOnStartup"] = "false",
                ["AI:Provider"] = "OpenAI",
                ["AI:Providers:OpenAI:Endpoint"] = "https://api.openai.com/v1",
                ["AI:Providers:OpenAI:Model"] = "gpt-5.6-terra",
                ["AI:Providers:OpenAI:ApiKey"] = "test-key"
            }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<NexaStoreDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<NexaStoreDbContext>>();
            services.AddDbContext<NexaStoreDbContext>(options => options.UseSqlite(_connection));
            services.RemoveAll<INexaAssistantService>();
            services.AddSingleton<INexaAssistantService, TestAssistantService>();
        });
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        _ = Services;
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NexaStoreDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        await scope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>().SeedAsync();
        CustomerId = await dbContext.Customers.Select(customer => customer.Id).SingleAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        await _connection.DisposeAsync();
    }

    private sealed class TestAssistantService : INexaAssistantService
    {
        public async IAsyncEnumerable<AssistantStreamUpdate> StreamResponseAsync(
            IReadOnlyList<AssistantMessageDto> messages,
            string responseStyle = AssistantResponseStyles.Balanced,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            yield return AssistantStreamUpdate.Tool("search_products", "Searched the live product catalog");
            yield return AssistantStreamUpdate.Delta("Test response");
        }
    }
}
