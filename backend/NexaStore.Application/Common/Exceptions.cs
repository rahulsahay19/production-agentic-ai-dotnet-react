namespace NexaStore.Application.Common;

public sealed class NotFoundException(string resourceName, object key)
    : Exception($"{resourceName} '{key}' was not found.");

public sealed class ApplicationValidationException(string message) : Exception(message);
