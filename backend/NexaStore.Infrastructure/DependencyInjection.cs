using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexaStore.Application.Abstractions;
using NexaStore.Application.AI;
using NexaStore.Infrastructure.Persistence;
using NexaStore.Infrastructure.Persistence.Repositories;
using NexaStore.Infrastructure.Persistence.Seeding;
using Microsoft.Extensions.AI;
using NexaStore.Infrastructure.AI;
using OllamaSharp;
using OpenAI;
using System.ClientModel;
using System.ClientModel.Primitives;
using NexaStore.Application.Knowledge;
using NexaStore.Infrastructure.Knowledge;

namespace NexaStore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAi(this IServiceCollection services, AiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        services.AddSingleton(options);

        if (!options.Enabled)
        {
            services.AddScoped<INexaAssistantService, DisabledNexaAssistantService>();
            return services;
        }

        services.AddSingleton<IChatClient>(provider =>
        {
            var selectedProvider = options.SelectedProvider;
            IChatClient providerClient;
            if (string.Equals(options.Provider, AiProviders.Ollama, StringComparison.OrdinalIgnoreCase))
            {
                var httpClient = new HttpClient
                {
                    BaseAddress = new Uri(selectedProvider.Endpoint),
                    Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds)
                };
                providerClient = new OllamaApiClient(httpClient, selectedProvider.Model);
            }
            else
            {
                var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds)
                };
                var openAiClient = new OpenAIClient(
                    new ApiKeyCredential(selectedProvider.ApiKey!),
                    new OpenAIClientOptions
                    {
                        Endpoint = new Uri(selectedProvider.Endpoint),
                        Transport = new HttpClientPipelineTransport(httpClient)
                    });
#pragma warning disable OPENAI001 // Responses adapter is the current supported path for reasoning + function tools.
                providerClient = openAiClient
                    .GetResponsesClient()
                    .AsIChatClient(selectedProvider.Model);
#pragma warning restore OPENAI001
            }

            return new ChatClientBuilder(providerClient)
                .UseFunctionInvocation(
                    provider.GetRequiredService<ILoggerFactory>(),
                    client =>
                    {
                        client.AllowConcurrentInvocation = false;
                        client.IncludeDetailedErrors = false;
                        client.MaximumConsecutiveErrorsPerRequest = 0;
                        client.MaximumIterationsPerRequest = 5;
                        client.TerminateOnUnknownCalls = true;
                    })
                .Build();
        });
        services.AddScoped<AiToolActivityTracker>();
        services.AddScoped<NexaStoreAiTools>();
        services.AddScoped<INexaAssistantService, NexaAssistantService>();
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        return services.AddInfrastructure(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(NexaStoreDbContext).Assembly.FullName);
                npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null);
                npgsql.UseVector();
            }));
    }

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDatabase)
    {
        services.AddDbContext<NexaStoreDbContext>(configureDatabase);
        services.AddScoped<DevelopmentDataSeeder>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<NexaStoreDbContext>());

        return services;
    }

    public static IServiceCollection AddKnowledge(
        this IServiceCollection services,
        KnowledgeOptions options,
        AiOptions aiOptions)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        var openAi = aiOptions.Providers.OpenAI;
        if (string.IsNullOrWhiteSpace(openAi.ApiKey))
        {
            throw new InvalidOperationException("OpenAI API key is required for knowledge-base embeddings.");
        }

        services.AddSingleton(options);
        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(_ =>
        {
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(aiOptions.RequestTimeoutSeconds) };
            var client = new OpenAIClient(
                new ApiKeyCredential(openAi.ApiKey),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(openAi.Endpoint),
                    Transport = new HttpClientPipelineTransport(httpClient)
                });
            return client.GetEmbeddingClient(options.EmbeddingModel)
                .AsIEmbeddingGenerator(options.EmbeddingDimensions);
        });
        services.AddScoped<IKnowledgeService, KnowledgeService>();
        return services;
    }
}
