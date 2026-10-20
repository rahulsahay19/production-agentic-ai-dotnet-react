using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using NexaStore.Application.Cart;
using NexaStore.Application.Catalog;
using NexaStore.Application.AI;

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
    public async Task AssistantStatus_ReturnsConfiguredLocalProvider()
    {
        var client = factory.CreateClient();

        var status = await client.GetFromJsonAsync<AssistantStatusDto>(
            "/api/assistant/status",
            CancellationToken.None);

        Assert.NotNull(status);
        Assert.True(status.Enabled);
        Assert.Equal("Ollama", status.Provider);
    }

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
        Assert.Contains("event: delta", body);
        Assert.Contains("Local test response", body);
        Assert.Contains("event: done", body);
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
