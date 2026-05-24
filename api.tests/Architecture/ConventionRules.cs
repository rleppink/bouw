using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Bouw.API.Tests.Architecture;

/// <summary>
/// Per-slice shape conventions: where endpoints and handlers live, and that
/// handlers stay free of HTTP plumbing.
/// </summary>
public sealed class ConventionRules
{
    /// <summary>
    /// #4 — endpoint mapping entrypoints live inside slices.
    /// </summary>
    [Fact]
    public void EndpointsResideUnderFeatures()
    {
        Classes()
            .That()
            .HaveName("Endpoint")
            .Should()
            .ResideInNamespaceMatching(ArchitectureFixture.FeaturesNamespacePattern)
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }

    /// <summary>#5 — feature service registrations live inside slices.</summary>
    [Fact]
    public void FeatureServiceRegistrationsResideUnderFeatures()
    {
        Classes()
            .That()
            .HaveName("FeatureServices")
            .Should()
            .ResideInNamespaceMatching(ArchitectureFixture.FeaturesNamespacePattern)
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }

    /// <summary>#6 — handlers are sealed (per ARCHITECTURE.md).</summary>
    [Fact]
    public void HandlersAreSealed()
    {
        Classes()
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .BeSealed()
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }

    /// <summary>#7 — handlers include the slice name in their type name.</summary>
    [Fact]
    public void HandlersUseSliceName()
    {
        foreach (var handler in ArchitectureFixture.FeatureHandlerTypes)
        {
            var sliceName = handler.Namespace!.Split('.').Last();

            Assert.Equal($"{sliceName}Handler", handler.Name);
        }
    }

    /// <summary>#8 — handlers live inside a slice, where business logic belongs.</summary>
    [Fact]
    public void HandlersResideUnderFeatures()
    {
        Classes()
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .ResideInNamespaceMatching(ArchitectureFixture.FeaturesNamespacePattern)
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }

    /// <summary>
    /// #9 (tune later) — handlers hold logic, not HTTP. They must not depend on
    /// ASP.NET MVC/HTTP plumbing; that belongs in Endpoint.cs. Revisit the banned
    /// set if handlers legitimately need e.g. IResult — see archunit-tests.md.
    /// </summary>
    [Fact]
    public void HandlersDoNotDependOnAspNetHttp()
    {
        Classes()
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .NotDependOnAny(
                Types()
                    .That()
                    .ResideInNamespaceMatching(ArchitectureFixture.AspNetHttpNamespacePattern)
            )
            .WithoutRequiringPositiveResults()
            .Check(ArchitectureFixture.Architecture);
    }
}
