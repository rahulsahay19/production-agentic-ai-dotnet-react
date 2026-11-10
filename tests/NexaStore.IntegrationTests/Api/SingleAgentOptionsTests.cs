using NexaStore.Infrastructure.AI;

namespace NexaStore.IntegrationTests.Api;

public sealed class SingleAgentOptionsTests
{
    [Fact]
    public void Validate_WithSupportedConfiguration_Succeeds()
    {
        var options = new SingleAgentOptions
        {
            Name = "NexaStore Support & Shopping Agent",
            MaximumToolIterations = 5
        };

        options.Validate();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Validate_WithUnsafeIterationLimit_Throws(int maximumToolIterations)
    {
        var options = new SingleAgentOptions
        {
            MaximumToolIterations = maximumToolIterations
        };

        var exception = Assert.Throws<InvalidOperationException>(options.Validate);

        Assert.Contains("between 1 and 10", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_WithDuplicateToolPermission_Throws()
    {
        var options = new SingleAgentOptions
        {
            AllowedTools = ["search_products", "search_products"]
        };

        var exception = Assert.Throws<InvalidOperationException>(options.Validate);

        Assert.Contains("unique", exception.Message, StringComparison.Ordinal);
    }
}
