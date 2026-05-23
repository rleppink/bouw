using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.Slices.SliceRuleDefinition;

namespace Bouw.API.Tests.Architecture;

/// <summary>
/// Slice-level invariants. A slice = one operation, identified by its full
/// namespace path under <c>Features</c>.
/// </summary>
public sealed class SliceRules
{
    /// <summary>
    /// #1 (THE invariant) — no slice references another slice. Keeps each slice
    /// deletable and comprehensible in isolation.
    /// </summary>
    [Fact]
    public void SlicesDoNotDependOnEachOther()
    {
        Slices()
            .Matching(ArchitectureFixture.FeaturesSlicePattern)
            .Should()
            .NotDependOnEachOther()
            .Check(ArchitectureFixture.Architecture);
    }

    /// <summary>
    /// #10 — the API is free of namespace dependency cycles. Complements the
    /// one-way layering rules at the whole-assembly level.
    /// </summary>
    [Fact]
    public void ApiIsFreeOfNamespaceCycles()
    {
        Slices()
            .Matching(ArchitectureFixture.ApiSlicePattern)
            .Should()
            .BeFreeOfCycles()
            .Check(ArchitectureFixture.Architecture);
    }
}
