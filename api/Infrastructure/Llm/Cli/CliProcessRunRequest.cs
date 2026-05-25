namespace Bouw.API.Infrastructure.Llm;

/// <summary>Input for starting a local CLI child process.</summary>
public sealed record CliProcessRunRequest(
    string Executable,
    string? Arguments,
    string StandardInput,
    string? WorkingDirectory
);
