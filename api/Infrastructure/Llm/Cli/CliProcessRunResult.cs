namespace Bouw.API.Infrastructure.Llm;

/// <summary>Captured output and exit status from a local CLI child process.</summary>
public sealed record CliProcessRunResult(
    int ExitCode,
    string StandardOutput,
    string? StandardError
);
