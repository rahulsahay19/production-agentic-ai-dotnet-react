using Microsoft.EntityFrameworkCore;
using NexaStore.Application.Abstractions;
using NexaStore.Domain.Customers;

namespace NexaStore.Infrastructure.Persistence.Repositories;

internal sealed class CustomerRepository(NexaStoreDbContext dbContext) : ICustomerRepository
{
    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Customers.AsNoTracking().SingleOrDefaultAsync(customer => customer.Id == id, cancellationToken);
}
