using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NexaStore.Application.Abstractions;
using NexaStore.Application.Cart;
using NexaStore.Application.Catalog;
using NexaStore.Application.Customers;
using NexaStore.Application.Configuration;
using NexaStore.Application.Inventory;
using NexaStore.Application.Orders;

namespace NexaStore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        CommerceOptions? commerceOptions = null)
    {
        commerceOptions ??= new CommerceOptions();
        commerceOptions.Validate();
        services.AddSingleton(commerceOptions);
        services.AddValidatorsFromAssemblyContaining<ProductService>();
        services.AddLogging();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IInventoryService, InventoryService>();
        return services;
    }
}
