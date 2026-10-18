namespace NexaStore.UnitTests;

public sealed class SolutionStructureTests
{
    [Fact]
    public void DomainAssembly_HasExpectedName()
    {
        var assemblyName = typeof(NexaStore.Domain.AssemblyMarker).Assembly.GetName().Name;

        Assert.Equal("NexaStore.Domain", assemblyName);
    }
}
