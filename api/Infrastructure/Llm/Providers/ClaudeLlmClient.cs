namespace Bouw.API.Infrastructure.Llm;

public sealed class ClaudeLlmClient(ICliProcessRunner processRunner) : CliLlmClient(processRunner)
{
    public override LlmBackendKind Backend => LlmBackendKind.Claude;

    protected override string Executable => "claude";

    protected override string BuildArguments(LlmPromptRequest request) =>
        $"-p --model {request.Model}";
}
