using System.Text.Json;

namespace Bouw.API.Infrastructure.Llm;

public sealed class OpenCodeLlmClient(ICliProcessRunner processRunner) : CliLlmClient(processRunner)
{
    public override LlmBackendKind Backend => LlmBackendKind.OpenCode;

    protected override string Executable => "opencode";

    protected override string BuildArguments(LlmPromptRequest request) =>
        $"run --format json --model {RequireSubscriptionName(request)}/{request.Model}";

    protected override string ExtractText(string standardOutput) =>
        LlmResponseTextExtractor.ExtractNdjsonTextEvents(standardOutput, TryGetTextPart);

    private static bool TryGetTextPart(JsonElement root, out string eventText)
    {
        eventText = string.Empty;

        if (!LlmResponseTextExtractor.HasStringProperty(root, "type", "text"))
        {
            return false;
        }

        if (!root.TryGetProperty("part", out var part))
        {
            return false;
        }

        return LlmResponseTextExtractor.TryGetStringProperty(part, "text", out eventText);
    }
}
