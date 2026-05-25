namespace Bouw.API.Infrastructure.Llm;

public sealed record LlmPromptRequest(
    string Prompt,
    string? WorkingDirectory,
    LlmBackendKind Backend,
    string? SubscriptionName,
    string Model,
    TimeSpan Timeout
);
