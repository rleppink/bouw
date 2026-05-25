namespace Bouw.API.Infrastructure.Llm;

public interface ILlmBackend
{
    Task<LlmPromptResponse> GenerateResponseAsync(LlmPromptRequest request, CancellationToken ct);
}
