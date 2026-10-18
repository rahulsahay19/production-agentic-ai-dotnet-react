namespace NexaStore.Domain.Catalog;

public sealed record ProductUsageClassification(
    bool IsAiReady = false,
    bool IsLocalLlmReady = false,
    bool IsDeveloperReady = false,
    bool IsCreatorReady = false,
    bool IsGamingReady = false,
    bool IsDockerReady = false);
