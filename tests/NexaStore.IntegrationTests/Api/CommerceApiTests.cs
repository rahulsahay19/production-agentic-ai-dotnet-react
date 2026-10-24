using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using NexaStore.Application.Cart;
using NexaStore.Application.Catalog;
using NexaStore.Application.AI;
using Microsoft.Extensions.DependencyInjection;
using NexaStore.Infrastructure.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;

namespace NexaStore.IntegrationTests.Api;

public sealed class CommerceApiTests(NexaStoreApiFactory factory) : IClassFixture<NexaStoreApiFactory>
{
    [Fact]
    public async Task Search_ReturnsStructuredFilteredProducts()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/products/search?ramMin=64&vramMin=16&aiReady=true",
            CancellationToken.None);
        var result = await response.Content.ReadFromJsonAsync<ProductSearchResult>(CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.All(result.Items, product =>
        {
            Assert.True(product.IsAiReady);
            Assert.True(product.StockQuantity >= 0);
        });
    }

    [Fact]
    public async Task AddCartItem_WithInvalidQuantity_ReturnsValidationProblem()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/cart/{factory.CustomerId}/items",
            new AddCartItemRequest(Guid.NewGuid(), 0),
            CancellationToken.None);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Contains("Quantity", problem.Errors.Keys);
    }

    [Fact]
    public async Task GetProduct_WhenMissing_ReturnsProblemDetails()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/products/{Guid.NewGuid()}", CancellationToken.None);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(CancellationToken.None);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Resource not found", problem?.Title);
        Assert.True(problem?.Extensions.ContainsKey("traceId"));
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health", CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(CancellationToken.None));
    }

    [Fact]
    public async Task AssistantStatus_ReturnsConfiguredCloudProvider()
    {
        var client = factory.CreateClient();

        var status = await client.GetFromJsonAsync<AssistantStatusDto>(
            "/api/assistant/status",
            CancellationToken.None);

        Assert.NotNull(status);
        Assert.True(status.Enabled);
        Assert.Equal("OpenAI", status.Provider);
        Assert.Equal("gpt-5.6-terra", status.Model);
        Assert.Contains("streaming", status.Capabilities);
        Assert.Contains("product-tools", status.Capabilities);
        Assert.Contains("balanced", status.ResponseStyles);
    }

    [Fact]
#pragma warning disable OPENAI001 // Verifies the intentionally selected Responses API adapter.
    public void OpenAiProvider_UsesResponsesApiAdapter()
    {
        var chatClient = factory.Services.GetRequiredService<IChatClient>();

        Assert.NotNull(chatClient.GetService<ResponsesClient>());
    }
#pragma warning restore OPENAI001

    [Fact]
    public async Task AssistantStream_ReturnsServerSentEvents()
    {
        var client = factory.CreateClient();
        var request = new AssistantChatRequest(
            [new AssistantMessageDto("user", "What is VRAM?")]);

        var response = await client.PostAsJsonAsync(
            "/api/assistant/chat/stream",
            request,
            CancellationToken.None);
        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/event-stream", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("event: started", body);
        Assert.Contains("\"responseStyle\":\"balanced\"", body);
        Assert.Contains("event: delta", body);
        Assert.Contains("event: tool", body);
        Assert.Contains("search_products", body);
        Assert.Contains("Test response", body);
        Assert.Contains("event: done", body);
        Assert.Contains("\"chunks\":1", body);
    }

    [Fact]
    public async Task AiProductTools_SearchSeededCatalogAndExposeOnlyReadOperations()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var tools = scope.ServiceProvider.GetRequiredService<NexaStoreAiTools>();

        var search = await tools.SearchProductsAsync(
            keyword: null,
            category: null,
            brand: null,
            maximumPrice: null,
            minimumRamGb: 64,
            minimumVramGb: 16,
            aiReady: true,
            localLlmReady: true,
            CancellationToken.None);
        var names = tools.CreateAll().Select(tool => tool.Name).ToArray();

        Assert.Null(search.Error);
        Assert.NotEmpty(search.Items);
        Assert.All(search.Items, product =>
        {
            Assert.True(product.AiReady);
            Assert.True(product.LocalLlmReady);
        });
        Assert.Equal(
            ["search_products", "get_product_details", "list_product_categories", "check_inventory"],
            names);
    }

    [Fact]
    public async Task AiInventoryTool_RejectsUnsafeQuantityWithoutReadingInventory()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var tools = scope.ServiceProvider.GetRequiredService<NexaStoreAiTools>();

        var result = await tools.CheckInventoryAsync(Guid.NewGuid(), 21, CancellationToken.None);

        Assert.False(result.IsAvailable);
        Assert.Equal("Quantity must be between 1 and 20.", result.Error);
    }

    [Fact]
    public async Task LiveHealth_DoesNotDependOnDatabaseChecks()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live", CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Preflight_FromConfiguredFrontend_ReturnsCorsHeaders()
    {
        var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Options, "/api/products");
        request.Headers.Add("Origin", "http://localhost:5173");
        request.Headers.Add("Access-Control-Request-Method", "GET");

        var response = await client.SendAsync(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(
            "http://localhost:5173",
            response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }
}
