using NexaStore.Application.AI;
using NexaStore.Infrastructure.AI;
using static NexaStore.Infrastructure.AI.JevDecisionService;

namespace NexaStore.IntegrationTests.Api;

public sealed class JevDecisionLayerTests
{
    private static readonly JevOptions Options = new();
    private readonly JevDecisionService _decisions = new(Options);

    [Theory]
    [InlineData("What is the difference between RAM and VRAM?", JevRoutes.General)]
    [InlineData("Summarize the return policy and cite the source.", JevRoutes.Knowledge)]
    [InlineData("Based on the local LLM guide, recommend an available laptop.", JevRoutes.Mixed)]
    [InlineData("Add two laptops to my cart.", JevRoutes.Restricted)]
    public void Decide_RoutesRequestsByEvidenceNeed(string request, string expectedRoute)
    {
        var decision = _decisions.Decide([new AssistantMessageDto("user", request)]);

        Assert.Equal(expectedRoute, decision.Route);
    }

    [Fact]
    public void Decide_RestrictedMutation_IsBlockedBeforeAgentExecution()
    {
        var decision = _decisions.Decide(
            [new AssistantMessageDto("user", "Place the order for me.")]);

        Assert.False(decision.Allowed);
        Assert.Equal(JevRisk.High, decision.Risk);
        Assert.Equal("unsupported-write-operation", decision.ReasonCode);
        Assert.Empty(decision.RequiredTools);
    }

    [Fact]
    public void Verify_MixedWorkflowWithToolsAndCitation_Passes()
    {
        var decision = _decisions.Decide(
            [new AssistantMessageDto("user", "Based on the local LLM guide, recommend an available laptop.")]);
        var context = new JevRunContext(Options);
        context.Begin(decision);
        context.RecordTool("search_knowledge_base");
        context.RecordTool("search_products");
        context.RecordTool("check_inventory");

        var verification = context.Verify(
            "The NexaBook fits the documented guidance. [Local LLM Buying Guide, p. 4]");

        Assert.True(verification.Passed);
        Assert.Equal("verified", verification.Status);
        Assert.Empty(verification.MissingEvidence);
    }

    [Fact]
    public void Verify_MissingRequiredEvidence_FailsClosed()
    {
        var decision = _decisions.Decide(
            [new AssistantMessageDto("user", "Summarize the return policy and cite the source.")]);
        var context = new JevRunContext(Options);
        context.Begin(decision);

        var verification = context.Verify("The return period is fourteen days.");

        Assert.False(verification.Passed);
        Assert.Contains("tool:search_knowledge_base", verification.MissingEvidence);
        Assert.Contains("document-citation", verification.MissingEvidence);
    }
}
