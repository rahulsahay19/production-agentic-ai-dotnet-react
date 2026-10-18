using Microsoft.AspNetCore.Mvc;
using NexaStore.Application.Abstractions;
using NexaStore.Application.Customers;
using NexaStore.Application.Orders;

namespace NexaStore.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Tags("Customers")]
public sealed class CustomersController(ICustomerService customers, IOrderService orders) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public Task<CustomerDto> GetCustomer(Guid id, CancellationToken cancellationToken) =>
        customers.GetCustomerAsync(id, cancellationToken);

    [HttpGet("{customerId:guid}/orders")]
    [ProducesResponseType<IReadOnlyCollection<OrderDto>>(StatusCodes.Status200OK)]
    public Task<IReadOnlyCollection<OrderDto>> GetOrders(Guid customerId, CancellationToken cancellationToken) =>
        orders.GetOrdersByCustomerAsync(customerId, cancellationToken);
}
