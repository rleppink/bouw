namespace Bouw.API.Infrastructure.Llm;

/// <summary>Runs a local operating-system child process for a CLI executable.</summary>
public interface ICliProcessRunner
{
    /// <summary>
    /// Starts the configured executable as a child process, writes stdin, and captures stdout,
    /// stderr, and exit code.
    /// </summary>
    Task<CliProcessRunResult> RunAsync(CliProcessRunRequest request, CancellationToken ct);
}
