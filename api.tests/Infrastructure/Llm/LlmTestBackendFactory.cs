using Bouw.API.Infrastructure.Llm;
using Microsoft.Extensions.DependencyInjection;

namespace Bouw.API.Tests.Infrastructure.Llm;

internal static class LlmTestBackendFactory
{
    public static ILlmBackend Create(FakeCliProcessRunner runner)
    {
        var services = new ServiceCollection();
        services.AddLlmInfrastructure();
        services.AddSingleton<ICliProcessRunner>(runner);

        return services.BuildServiceProvider().GetRequiredService<ILlmBackend>();
    }
}
