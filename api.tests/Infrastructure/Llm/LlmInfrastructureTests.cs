using Bouw.API.Infrastructure.Llm;

namespace Bouw.API.Tests.Infrastructure.Llm;

public sealed class LlmInfrastructureTests
{
    [Fact]
    public async Task GenerateResponseAsyncPassesPromptThroughStandardInput()
    {
        var runner = new FakeCliProcessRunner(
            new CliProcessRunResult(
                0,
                "{\"type\":\"item.completed\",\"item\":{\"type\":\"agent_message\",\"text\":\"answer\"}}",
                "diagnostic"
            )
        );
        var backend = LlmTestBackendFactory.Create(runner);

        var response = await backend.GenerateResponseAsync(
            CodexRequest("prompt text"),
            CancellationToken.None
        );

        Assert.NotNull(runner.Request);
        Assert.Equal("prompt text", runner.Request.StandardInput);
        Assert.Equal("/tmp/work", runner.Request.WorkingDirectory);
        Assert.Equal("answer", response.Text);
        Assert.Equal(LlmBackendKind.Codex, response.Backend);
        Assert.Equal(0, response.ExitCode);
        Assert.Equal("diagnostic", response.StandardError);
    }

    [Fact]
    public async Task ClaudeUsesPrintModeAndModel()
    {
        var runner = new FakeCliProcessRunner(new CliProcessRunResult(0, "answer", null));
        var backend = LlmTestBackendFactory.Create(runner);

        await backend.GenerateResponseAsync(
            Request(LlmBackendKind.Claude, "anthropic", "sonnet", "prompt"),
            CancellationToken.None
        );

        Assert.NotNull(runner.Request);
        Assert.Equal("claude", runner.Request.Executable);
        Assert.Equal("-p --model sonnet", runner.Request.Arguments);
    }

    [Fact]
    public async Task OpenCodeUsesNdjsonModeSubscriptionAndModel()
    {
        const string standardOutput = """
            {"type":"text","part":{"type":"text","text":"answer"}}
            """;
        var runner = new FakeCliProcessRunner(new CliProcessRunResult(0, standardOutput, null));
        var backend = LlmTestBackendFactory.Create(runner);

        await backend.GenerateResponseAsync(
            Request(LlmBackendKind.OpenCode, "openai", "gpt-5", "prompt"),
            CancellationToken.None
        );

        Assert.NotNull(runner.Request);
        Assert.Equal("opencode", runner.Request.Executable);
        Assert.Equal("run --format json --model openai/gpt-5", runner.Request.Arguments);
    }

    [Fact]
    public async Task PiUsesPrintModeNoSessionSubscriptionAndModel()
    {
        var runner = new FakeCliProcessRunner(new CliProcessRunResult(0, "answer", null));
        var backend = LlmTestBackendFactory.Create(runner);

        await backend.GenerateResponseAsync(
            Request(LlmBackendKind.Pi, "google", "gemini-2.5-pro", "prompt"),
            CancellationToken.None
        );

        Assert.NotNull(runner.Request);
        Assert.Equal("pi", runner.Request.Executable);
        Assert.Equal(
            "--print --no-session --model google/gemini-2.5-pro",
            runner.Request.Arguments
        );
    }

    [Fact]
    public async Task OpenCodeExtractsMultilineNdjsonTextEvents()
    {
        const string standardOutput = """
            {"type":"step_start","part":{"type":"step-start"}}
            {"type":"text","part":{"type":"text","text":"line one\nline two"}}
            {"type":"step_finish","part":{"type":"step-finish"}}
            """;
        var backend = LlmTestBackendFactory.Create(
            new FakeCliProcessRunner(new CliProcessRunResult(0, standardOutput, null))
        );

        var response = await backend.GenerateResponseAsync(
            Request(LlmBackendKind.OpenCode, "openai", "gpt-5", "prompt"),
            CancellationToken.None
        );

        Assert.Equal("line one\nline two", response.Text);
    }

    [Fact]
    public async Task CodexExtractsMultilineNdjsonAgentMessages()
    {
        const string standardOutput = """
            {"type":"thread.started","thread_id":"id"}
            {"type":"item.completed","item":{"type":"agent_message","text":"line one\nline two"}}
            {"type":"turn.completed","usage":{"input_tokens":1}}
            """;
        var backend = LlmTestBackendFactory.Create(
            new FakeCliProcessRunner(new CliProcessRunResult(0, standardOutput, null))
        );

        var response = await backend.GenerateResponseAsync(
            CodexRequest("prompt"),
            CancellationToken.None
        );

        Assert.Equal("line one\nline two", response.Text);
    }

    [Fact]
    public async Task GenerateResponseAsyncThrowsWhenModelIsMissing()
    {
        var backend = LlmTestBackendFactory.Create(
            new FakeCliProcessRunner(new CliProcessRunResult(0, "unused", null))
        );

        var exception = await Assert.ThrowsAsync<LlmBackendException>(() =>
            backend.GenerateResponseAsync(
                Request(LlmBackendKind.Codex, "openai", "", "prompt"),
                CancellationToken.None
            )
        );

        Assert.Contains("model", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GenerateResponseAsyncThrowsWhenSubscriptionIsRequiredButMissing()
    {
        var backend = LlmTestBackendFactory.Create(
            new FakeCliProcessRunner(new CliProcessRunResult(0, "unused", null))
        );

        var exception = await Assert.ThrowsAsync<LlmBackendException>(() =>
            backend.GenerateResponseAsync(
                Request(LlmBackendKind.OpenCode, null, "gpt-5", "prompt"),
                CancellationToken.None
            )
        );

        Assert.Contains("subscription", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GenerateResponseAsyncThrowsWhenProcessReturnsNonZeroExitCode()
    {
        var backend = LlmTestBackendFactory.Create(
            new FakeCliProcessRunner(new CliProcessRunResult(23, "unused", "failed"))
        );

        var exception = await Assert.ThrowsAsync<LlmBackendException>(() =>
            backend.GenerateResponseAsync(CodexRequest("prompt"), CancellationToken.None)
        );

        Assert.Contains("23", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GenerateResponseAsyncThrowsClearTimeoutFailure()
    {
        var runner = new FakeCliProcessRunner(
            async (_, ct) =>
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, ct).ConfigureAwait(true);
                return new CliProcessRunResult(0, "unused", null);
            }
        );
        var backend = LlmTestBackendFactory.Create(runner);

        var exception = await Assert.ThrowsAsync<LlmBackendException>(() =>
            backend.GenerateResponseAsync(
                CodexRequest("prompt", TimeSpan.FromMilliseconds(1)),
                CancellationToken.None
            )
        );

        Assert.Contains("timed out", exception.Message, StringComparison.Ordinal);
    }

    private static LlmPromptRequest CodexRequest(string prompt, TimeSpan? timeout = null) =>
        Request(LlmBackendKind.Codex, "openai", "gpt-5", prompt, timeout);

    private static LlmPromptRequest Request(
        LlmBackendKind backend,
        string? subscriptionName,
        string model,
        string prompt,
        TimeSpan? timeout = null
    ) =>
        new(
            prompt,
            "/tmp/work",
            backend,
            subscriptionName,
            model,
            timeout ?? TimeSpan.FromSeconds(30)
        );
}
