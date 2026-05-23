using ArchUnitNET.Loader;

namespace Bouw.API.Tests.Architecture;

/// <summary>
/// Loads the compiled <c>Bouw.API</c> assembly once for all architecture rules
/// and exposes the namespace/slice patterns the rules bind to. ArchUnitNET
/// inspects IL, so these rules describe the shape the assembly must keep as
/// slices land — today most pass vacuously because the namespaces hold no types
/// yet (see <c>api/docs/archunit-tests.md</c>).
/// </summary>
internal static class ArchitectureFixture
{
    // Anchored-prefix regexes: match the namespace itself AND any descendant
    // (e.g. Features and Features.Workflows.CreateWorkflow), but not a sibling
    // like FeaturesFoo. Used with ResideInNamespaceMatching, since the plain
    // ResideInNamespace predicate matches a namespace exactly.
    public const string FeaturesNamespacePattern = @"^Bouw\.API\.Features(\.|$)";
    public const string PersistenceNamespacePattern = @"^Bouw\.API\.Persistence(\.|$)";
    public const string InfrastructureNamespacePattern = @"^Bouw\.API\.Infrastructure(\.|$)";

    // ASP.NET HTTP/MVC plumbing handlers must stay clear of (rule #8). Tune the
    // banned set as real handlers land — see archunit-tests.md.
    public const string AspNetHttpNamespacePattern = @"^Microsoft\.AspNetCore\.(Mvc|Http)(\.|$)";

    // Slice identity = the full path after Features. "(**)" captures the whole
    // remainder, so Workflows.CreateWorkflow and Workflows.EditWorkflow are
    // distinct slices: the grouping folder (Workflows) is NOT a boundary.
    public const string FeaturesSlicePattern = "Bouw.API.Features.(**)";

    // Whole-API slicing for layer-level cycle freedom (rule #10).
    public const string ApiSlicePattern = "Bouw.API.(**)";

    /// <summary>The architecture under test, loaded once from the API assembly.</summary>
    public static readonly ArchUnitNET.Domain.Architecture Architecture =
        new ArchLoader()
            .LoadAssemblies(typeof(Bouw.API.Infrastructure.IEndpoint).Assembly)
            .Build();
}
