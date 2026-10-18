using FluentValidation;

namespace NexaStore.Application.Orders;

public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(request => request.CustomerId).NotEmpty();
        RuleFor(request => request.ShippingAddress).NotNull().SetValidator(new AddressDtoValidator());
        RuleFor(request => request.BillingAddress).NotNull().SetValidator(new AddressDtoValidator());
    }
}

public sealed class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(address => address.RecipientName).NotEmpty().MaximumLength(150);
        RuleFor(address => address.Line1).NotEmpty().MaximumLength(200);
        RuleFor(address => address.Line2).MaximumLength(200);
        RuleFor(address => address.City).NotEmpty().MaximumLength(100);
        RuleFor(address => address.StateOrProvince).NotEmpty().MaximumLength(100);
        RuleFor(address => address.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(address => address.CountryCode).NotEmpty().Length(2);
        RuleFor(address => address.Phone).MaximumLength(30);
    }
}
