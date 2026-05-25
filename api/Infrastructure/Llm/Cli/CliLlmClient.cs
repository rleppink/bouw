namespace Bouw.API.Infrastructure.Llm;

public abstract class CliLlmClient(ICliProcessRunner processRunner)
{
    public abstract LlmBackendKind Backend { get; }

    public async Task<LlmPromptResponse> GenerateResponseAsync(
        LlmPromptRequest request,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateRequest(request);

        using var timeoutCts = new CancellationTokenSource(request.Timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

        CliProcessRunResult result;
        try
        {
            result = await processRunner
                .RunAsync(
                    new CliProcessRunRequest(
                        this.Executable,
                        this.BuildArguments(request),
                        request.Prompt,
                        request.WorkingDirectory
                    ),
                    linkedCts.Token
                )
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException exception) when (timeoutCts.IsCancellationRequested)
        {
            throw new LlmBackendException(
                $"LLM backend '{this.Backend}' timed out after {request.Timeout.TotalSeconds:N0} seconds.",
                exception
            );
        }
        catch (OperationCanceledException exception)
        {
            throw new LlmBackendException(
                $"LLM backend '{this.Backend}' invocation was cancelled.",
                exception
            );
        }

        if (result.ExitCode != 0)
        {
            throw new LlmBackendException(
                $"LLM backend '{this.Backend}' exited with code {result.ExitCode}."
            );
        }

        return new LlmPromptResponse(
            this.ExtractText(result.StandardOutput),
            this.Backend,
            result.ExitCode,
            result.StandardError
        );
    }

    protected abstract string Executable { get; }

    protected abstract string BuildArguments(LlmPromptRequest request);

    protected virtual string ExtractText(string standardOutput) => standardOutput;

    protected static string RequireSubscriptionName(LlmPromptRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.SubscriptionName))
        {
            throw new LlmBackendException(
                $"LLM backend '{request.Backend}' requires a subscription name."
            );
        }

        return request.SubscriptionName;
    }

    private static void ValidateRequest(LlmPromptRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Model))
        {
            throw new LlmBackendException("LLM model is required.");
        }

        if (request.Timeout <= TimeSpan.Zero)
        {
            throw new LlmBackendException("LLM timeout must be greater than 0.");
        }
    }
}
