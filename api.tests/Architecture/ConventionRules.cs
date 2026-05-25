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

    /// <summary>#7 — handlers use the conventional per-slice type and file name.</summary>
    [Fact]
    public void HandlersAreNamedHandler()
    {
        foreach (var handler in ArchitectureFixture.FeatureHandlerTypes)
        {
            Assert.Equal("Handler", handler.Name);
        }
    }

    [Fact]
    public void HandlerFilesAreNamedHandler()
    {
        var featuresDirectory = Path.Combine(FindRepositoryRoot(), "api", "Features");

        if (!Directory.Exists(featuresDirectory))
        {
            return;
        }

        var handlerFiles = Directory.GetFiles(
            featuresDirectory,
            "*Handler.cs",
            SearchOption.AllDirectories
        );

        foreach (var handlerFile in handlerFiles)
        {
            Assert.Equal("Handler.cs", Path.GetFileName(handlerFile));
        }

        var sliceDirectories = Directory
            .GetDirectories(featuresDirectory, "*", SearchOption.AllDirectories)
            .Where(directory =>
                File.Exists(Path.Combine(directory, "Endpoint.cs"))
                || File.Exists(Path.Combine(directory, "FeatureServices.cs"))
            );

        foreach (var sliceDirectory in sliceDirectories)
        {
            Assert.True(
                File.Exists(Path.Combine(sliceDirectory, "Handler.cs")),
                $"Slice directory must contain Handler.cs: {sliceDirectory}"
            );
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

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "api", "Features")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find repository root.");
    }
}
