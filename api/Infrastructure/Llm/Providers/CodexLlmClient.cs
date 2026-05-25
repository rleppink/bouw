using System.Text.Json;

namespace Bouw.API.Infrastructure.Llm;

public sealed class CodexLlmClient(ICliProcessRunner processRunner) : CliLlmClient(processRunner)
{
    public override LlmBackendKind Backend => LlmBackendKind.Codex;

    protected override string Executable => "codex";

    protected override string BuildArguments(LlmPromptRequest request) =>
        $"exec --json --model {request.Model} -";

    protected override string ExtractText(string standardOutput) =>
        LlmResponseTextExtractor.ExtractNdjsonTextEvents(standardOutput, TryGetAgentMessageText);

    private static bool TryGetAgentMessageText(JsonElement root, out string eventText)
    {
        eventText = string.Empty;

        if (!LlmResponseTextExtractor.HasStringProperty(root, "type", "item.completed"))
        {
            return false;
        }

        if (!root.TryGetProperty("item", out var item))
        {
            return false;
        }

        return LlmResponseTextExtractor.HasStringProperty(item, "type", "agent_message")
            && LlmResponseTextExtractor.TryGetStringProperty(item, "text", out eventText);
    }
}
