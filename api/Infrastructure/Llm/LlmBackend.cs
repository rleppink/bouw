namespace Bouw.API.Infrastructure.Llm;

public sealed class LlmBackend(IEnumerable<CliLlmClient> clients) : ILlmBackend
{
    public async Task<LlmPromptResponse> GenerateResponseAsync(
        LlmPromptRequest request,
        CancellationToken ct
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        var client = clients.SingleOrDefault(client => client.Backend == request.Backend);

        if (client is null)
        {
            throw new LlmBackendException($"LLM backend '{request.Backend}' is not registered.");
        }

        return await client.GenerateResponseAsync(request, ct).ConfigureAwait(false);
    }
}
