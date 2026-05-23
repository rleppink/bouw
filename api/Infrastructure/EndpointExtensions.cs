using System.Reflection;

namespace Bouw.API.Infrastructure;

/// <summary>
/// Discovers every <see cref="IEndpoint"/> in this assembly and maps it. This is
/// the single registration point: slices self-register by implementing
/// <see cref="IEndpoint"/>, so <c>Program.cs</c> stays untouched as slices land.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Reflection-scans the API assembly for concrete <see cref="IEndpoint"/>
    /// implementors and invokes each one's static <see cref="IEndpoint.Map"/>.
    /// </summary>
    public static void MapFeatures(this IEndpointRouteBuilder app)
    {
        foreach (var type in typeof(EndpointExtensions).Assembly.GetTypes())
        {
            if (
                type is { IsClass: true, IsAbstract: false }
                && typeof(IEndpoint).IsAssignableFrom(type)
            )
            {
                var map = type.GetMethod(
                    nameof(IEndpoint.Map),
                    BindingFlags.Public | BindingFlags.Static
                );
                map?.Invoke(obj: null, parameters: [app]);
            }
        }
    }
}
