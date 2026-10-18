using Microsoft.AspNetCore.Mvc;
using NexaStore.Application.Abstractions;
using NexaStore.Application.Catalog;

namespace NexaStore.Api.Controllers;

[ApiController]
[Route("api/products")]
[Tags("Catalog")]
public sealed class ProductsController(IProductService products) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ProductSearchResult>(StatusCodes.Status200OK)]
    public Task<ProductSearchResult> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        products.GetProductsAsync(new ProductSearchRequest(Page: page, PageSize: pageSize), cancellationToken);

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProductDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<ProductDetailsDto> GetProduct(Guid id, CancellationToken cancellationToken) =>
        products.GetProductByIdAsync(id, cancellationToken);

    [HttpGet("search")]
    [ProducesResponseType<ProductSearchResult>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public Task<ProductSearchResult> Search(
        [FromQuery] string? keyword,
        [FromQuery] string? category,
        [FromQuery] string? brand,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int? ramMin,
        [FromQuery] int? vramMin,
        [FromQuery] bool? aiReady,
        [FromQuery] bool? localLlmReady,
        [FromQuery] bool? developerReady,
        [FromQuery] bool? creatorReady,
        [FromQuery] bool? gamingReady,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        products.GetProductsAsync(new ProductSearchRequest(
            keyword, category, brand, minPrice, maxPrice, ramMin, vramMin,
            aiReady, localLlmReady, developerReady, creatorReady, gamingReady, page, pageSize), cancellationToken);
}
