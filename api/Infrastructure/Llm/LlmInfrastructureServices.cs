using Microsoft.Extensions.DependencyInjection;

namespace Bouw.API.Infrastructure.Llm;

public static class LlmInfrastructureServices
{
    public static void AddLlmInfrastructure(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ICliProcessRunner, CliProcessRunner>();
        services.AddSingleton<CliLlmClient, ClaudeLlmClient>();
        services.AddSingleton<CliLlmClient, OpenCodeLlmClient>();
        services.AddSingleton<CliLlmClient, CodexLlmClient>();
        services.AddSingleton<CliLlmClient, PiLlmClient>();
        services.AddSingleton<ILlmBackend, LlmBackend>();
    }
}
