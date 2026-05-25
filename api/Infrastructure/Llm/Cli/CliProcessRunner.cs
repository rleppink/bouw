using System.Diagnostics;
using System.Text;

namespace Bouw.API.Infrastructure.Llm;

/// <summary>Production runner for local CLI child processes.</summary>
public sealed class CliProcessRunner : ICliProcessRunner
{
    public async Task<CliProcessRunResult> RunAsync(
        CliProcessRunRequest request,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = request.Executable,
                Arguments = request.Arguments ?? string.Empty,
                WorkingDirectory = request.WorkingDirectory ?? string.Empty,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardInputEncoding = Encoding.UTF8,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                UseShellExecute = false,
            },
        };

        process.Start();

        var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = process.StandardError.ReadToEndAsync(ct);

        await process
            .StandardInput.WriteAsync(request.StandardInput.AsMemory(), ct)
            .ConfigureAwait(false);
        await process.StandardInput.FlushAsync(ct).ConfigureAwait(false);
        process.StandardInput.Close();

        try
        {
            await process.WaitForExitAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            KillProcess(process);
            throw;
        }

        var stdout = await stdoutTask.ConfigureAwait(false);
        var stderr = await stderrTask.ConfigureAwait(false);

        return new CliProcessRunResult(
            process.ExitCode,
            stdout,
            string.IsNullOrEmpty(stderr) ? null : stderr
        );
    }

    private static void KillProcess(Process process)
    {
        if (process.HasExited)
        {
            return;
        }

        process.Kill(entireProcessTree: true);
    }
}
