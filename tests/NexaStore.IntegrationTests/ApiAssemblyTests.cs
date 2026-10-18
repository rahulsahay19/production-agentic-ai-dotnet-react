namespace NexaStore.IntegrationTests;

public sealed class ApiAssemblyTests
{
    [Fact]
    public void ApiAssembly_HasExpectedName()
    {
        var assemblyName = typeof(Program).Assembly.GetName().Name;

        Assert.Equal("NexaStore.Api", assemblyName);
    }
}
