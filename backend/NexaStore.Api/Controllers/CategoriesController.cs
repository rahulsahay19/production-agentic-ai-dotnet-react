using Microsoft.AspNetCore.Mvc;
using NexaStore.Application.Abstractions;
using NexaStore.Application.Catalog;

namespace NexaStore.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Tags("Catalog")]
public sealed class CategoriesController(IProductService products) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<CategoryDto>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyCollection<CategoryDto>> GetCategories(CancellationToken cancellationToken) =>
        products.GetCategoriesAsync(cancellationToken);
}
