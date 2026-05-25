namespace Bouw.API.Infrastructure.Llm;

public sealed class PiLlmClient(ICliProcessRunner processRunner) : CliLlmClient(processRunner)
{
    public override LlmBackendKind Backend => LlmBackendKind.Pi;

    protected override string Executable => "pi";

    protected override string BuildArguments(LlmPromptRequest request) =>
        $"--print --no-session --model {RequireSubscriptionName(request)}/{request.Model}";
}
