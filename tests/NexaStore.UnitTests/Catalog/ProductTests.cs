using NexaStore.Domain.Catalog;
using NexaStore.Domain.Common;

namespace NexaStore.UnitTests.Catalog;

public sealed class ProductTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void SetTechnicalSpecifications_WhenMaximumRamIsBelowInstalledRam_Throws()
    {
        Assert.Throws<DomainException>(() => new ProductTechnicalDetails(ramGb: 64, maxRamGb: 32));
    }

    [Fact]
    public void AddTag_NormalizesAndPreventsDuplicates()
    {
        var product = CreateProduct();

        product.AddTag(" Local AI ");
        product.AddTag("LOCAL AI");

        var tag = Assert.Single(product.Tags);
        Assert.Equal("local ai", tag.Name);
    }

    [Fact]
    public void AddImage_WhenNewPrimaryIsAdded_MaintainsSinglePrimaryImage()
    {
        var product = CreateProduct();

        product.AddImage("/first.webp", "First", true);
        product.AddImage("/second.webp", "Second", true);

        Assert.Equal(2, product.Images.Count);
        var primary = Assert.Single(product.Images, image => image.IsPrimary);
        Assert.Equal("/second.webp", primary.Url);
    }

    private static Product CreateProduct() => new(
        "NX-BOOK-1",
        "NexaBook Pro",
        "nexabook-pro",
        "NexaStore",
        Guid.NewGuid(),
        "Developer laptop",
        "A powerful developer laptop.",
        150_000m,
        "INR",
        Now);
}
