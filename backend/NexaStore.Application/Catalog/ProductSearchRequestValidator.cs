using FluentValidation;

namespace NexaStore.Application.Catalog;

public sealed class ProductSearchRequestValidator : AbstractValidator<ProductSearchRequest>
{
    public ProductSearchRequestValidator()
    {
        RuleFor(request => request.Keyword).MaximumLength(200);
        RuleFor(request => request.Category).MaximumLength(120);
        RuleFor(request => request.Brand).MaximumLength(100);
        RuleFor(request => request.MinimumPrice).GreaterThanOrEqualTo(0).When(request => request.MinimumPrice.HasValue);
        RuleFor(request => request.MaximumPrice).GreaterThanOrEqualTo(0).When(request => request.MaximumPrice.HasValue);
        RuleFor(request => request.MaximumPrice)
            .GreaterThanOrEqualTo(request => request.MinimumPrice)
            .When(request => request.MinimumPrice.HasValue && request.MaximumPrice.HasValue);
        RuleFor(request => request.MinimumRamGb).GreaterThanOrEqualTo(0).When(request => request.MinimumRamGb.HasValue);
        RuleFor(request => request.MinimumVramGb).GreaterThanOrEqualTo(0).When(request => request.MinimumVramGb.HasValue);
        RuleFor(request => request.Page).GreaterThanOrEqualTo(1);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 100);
    }
}
