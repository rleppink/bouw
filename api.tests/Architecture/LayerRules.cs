using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Bouw.API.Tests.Architecture;

/// <summary>
/// Layer dependencies point one way: <c>Features</c> → <c>Persistence</c> /
/// <c>Infrastructure</c>, never back.
/// </summary>
public sealed class LayerRules
{
    /// <summary>#2 — Persistence must not depend on any slice.</summary>
    [Fact]
    public void PersistenceDoesNotDependOnFeatures()
    {
        Types()
            .That()
            .ResideInNamespaceMatching(ArchitectureFixture.PersistenceNamespacePattern)
            .Should()
            .NotDependOnAny(
                Types()
                    .That()
                    .ResideInNamespaceMatching(ArchitectureFixture.FeaturesNamespacePattern)
            )
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }

    /// <summary>#2 — Infrastructure must not depend on any slice.</summary>
    [Fact]
    public void InfrastructureDoesNotDependOnFeatures()
    {
        Types()
            .That()
            .ResideInNamespaceMatching(ArchitectureFixture.InfrastructureNamespacePattern)
            .Should()
            .NotDependOnAny(
                Types()
                    .That()
                    .ResideInNamespaceMatching(ArchitectureFixture.FeaturesNamespacePattern)
            )
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }
}
