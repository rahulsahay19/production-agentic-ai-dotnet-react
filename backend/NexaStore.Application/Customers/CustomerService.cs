using NexaStore.Application.Abstractions;
using NexaStore.Application.Common;
using NexaStore.Domain.Customers;

namespace NexaStore.Application.Customers;

public sealed class CustomerService(ICustomerRepository customers) : ICustomerService
{
    public async Task<CustomerDto> GetCustomerAsync(Guid id, CancellationToken cancellationToken)
    {
        var customer = await customers.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Customer), id);
        return customer.ToDto();
    }
}
