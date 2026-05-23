namespace Bouw.API.Infrastructure;

/// <summary>
/// Marks a vertical slice's HTTP entrypoint. Each slice's <c>Endpoint.cs</c>
/// implements this; <see cref="EndpointExtensions.MapFeatures"/> reflection-scans
/// the assembly and wires every implementor, so adding a slice never edits
/// <c>Program.cs</c>.
/// </summary>
public interface IEndpoint
{
    /// <summary>Maps this slice's route(s) onto the application.</summary>
    static abstract void Map(IEndpointRouteBuilder app);
}
