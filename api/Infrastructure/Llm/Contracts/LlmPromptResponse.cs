namespace Bouw.API.Infrastructure.Llm;

public sealed record LlmPromptResponse(
    string Text,
    LlmBackendKind Backend,
    int ExitCode,
    string? StandardError
);
