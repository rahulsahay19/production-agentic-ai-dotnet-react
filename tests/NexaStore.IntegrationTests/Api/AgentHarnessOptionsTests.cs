using NexaStore.Infrastructure.AI;

namespace NexaStore.IntegrationTests.Api;

public sealed class AgentHarnessOptionsTests
{
    [Fact]
    public void Validate_WithProductionSafeDefaults_Succeeds()
    {
        var options = new AgentHarnessOptions();

        options.Validate();
    }

    [Theory]
    [InlineData(4)]
    [InlineData(601)]
    public void Validate_WithUnsafeRunDuration_Throws(int durationSeconds)
    {
        var options = new AgentHarnessOptions
        {
            MaximumRunDurationSeconds = durationSeconds
        };

        var exception = Assert.Throws<InvalidOperationException>(options.Validate);

        Assert.Contains("between 5 and 600", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Validate_WithUnsafeConversationLimit_Throws(int messageLimit)
    {
        var options = new AgentHarnessOptions
        {
            MaximumConversationMessages = messageLimit
        };

        var exception = Assert.Throws<InvalidOperationException>(options.Validate);

        Assert.Contains("between 1 and 100", exception.Message, StringComparison.Ordinal);
    }
}
