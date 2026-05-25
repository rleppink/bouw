using Bouw.API.Infrastructure.Llm;

namespace Bouw.API.Tests.Infrastructure.Llm;

internal sealed class FakeCliProcessRunner : ICliProcessRunner
{
    private readonly Func<CliProcessRunRequest, CancellationToken, Task<CliProcessRunResult>> run;

    public FakeCliProcessRunner(CliProcessRunResult result)
        : this((_, _) => Task.FromResult(result)) { }

    public FakeCliProcessRunner(
        Func<CliProcessRunRequest, CancellationToken, Task<CliProcessRunResult>> run
    )
    {
        this.run = run;
    }

    public CliProcessRunRequest? Request { get; private set; }

    public Task<CliProcessRunResult> RunAsync(CliProcessRunRequest request, CancellationToken ct)
    {
        this.Request = request;
        return this.run(request, ct);
    }
}
