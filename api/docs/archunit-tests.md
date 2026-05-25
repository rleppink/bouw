# ArchUnitNET architecture tests

How `Bouw.API`'s architecture is enforced as executable tests, the full catalogue
of candidate rules (with verdicts), and the caveats that matter while the
codebase is greenfield.

> **Scope.** [ArchUnitNET](https://archunit.net) inspects a *compiled assembly*
> (IL, via Mono.Cecil) and asserts properties of its types and their
> dependencies. The rules live in `api.tests/Architecture/` and run as ordinary
> xUnit tests (`dotnet test`). They are the executable form of the **Enforcement**
> section of `ARCHITECTURE.md`.

---

## The single most important caveat: vacuous rules

ArchUnitNET checks types in a compiled assembly, so a rule whose subject set is
empty (e.g. "all classes ending in `Handler`") has **nothing to check** and
would, by ArchUnitNET's default, *fail* — it requires positive results, not
merely the absence of violations.

We deliberately relax that with **`.WithoutRequiringPositiveResults()`** on every
type/class rule. The effect:

- **When a rule has no matching types:** it passes vacuously — it is a dormant
  guardrail.
- **As slices land:** the moment a `Handler` (or slice) appears,
  the rule binds to it and **fails closed** on any violation.

The one trade-off to know: a rule whose predicate matches *nothing* will pass
silently. So a typo'd namespace pattern won't announce itself — it just won't
guard anything. This is acceptable in the greenfield phase (these rules are
*meant* to match nothing yet) and was de-risked by the red-green check below.
A reasonable later hardening: once a layer is guaranteed non-empty, drop
`WithoutRequiringPositiveResults()` from its rule to regain the
"is-this-actually-testing-something" assurance.

Slice rules (#1, #11) do **not** carry the flag — `NotDependOnEachOther` and
`BeFreeOfCycles` pass cleanly over an empty/acyclic slice set.

---

## What's wired up

| File                                            | Contents                                                                                                                                     |
|-------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------|
| `api/Program.cs`                                | Explicitly calls each slice's service and endpoint extension methods.                                                                        |
| `api.tests/Architecture/ArchitectureFixture.cs` | Loads the API assembly once; holds the namespace/slice patterns.                                                                             |
| `api.tests/Architecture/SliceRules.cs`          | #1, #11.                                                                                                                                     |
| `api.tests/Architecture/LayerRules.cs`          | #2 (×2).                                                                                                                                     |
| `api.tests/Architecture/ConventionRules.cs`     | #5, #6, #7, #8, #9.                                                                                                                          |

Package: `TngTech.ArchUnitNET.xUnit` 0.13.3 (xUnit v2), via central package
management in `Directory.Packages.props`.

---

## The catalogue

Legend — **✅ implemented** · **🟡 optional / later** · **❌ not via ArchUnitNET**.
*Could* = expressible in ArchUnitNET; *Would* = worth the rule; *Would not* = why skip.

### Core invariants (from ARCHITECTURE.md)

| # | Rule                                                                        | Could?       | Verdict | Mechanism / notes                                                                                                                                                                                                                                                                                                                                                                                                                        |
|---|-----------------------------------------------------------------------------|--------------|---------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 1 | **No slice depends on another slice**                                       | Yes          | ✅      | `Slices().Matching("Bouw.API.Features.(**)").Should().NotDependOnEachOther()`. THE invariant. `(**)` captures the whole path after `Features.`, so `Workflows.CreateWorkflow` and `Workflows.EditWorkflow` are **distinct** slices — the grouping folder is *not* a boundary, matching the doc.                                                                                                                                          |
| 2 | **Dependencies one-way: Features → Persistence/Infrastructure, never back** | Yes          | ✅      | Two rules: `Persistence` ⇏ `Features` and `Infrastructure` ⇏ `Features`, via `NotDependOnAny`.                                                                                                                                                                                                                                                                                                                                           |
| 3 | **Every slice endpoint is mapped by `Program`**                             | No (runtime) | ❌→🟡   | ArchUnitNET checks static type deps, not the runtime endpoint table. With explicit startup wiring, reachability is best covered by a runtime test (`WebApplicationFactory<Program>` + `EndpointDataSource`). Feasible (`Program` is public) but out of scope. **Static substitutes we ship:** rules #5 and #6. |

### High-value additions (beyond the doc) — ✅ implemented

| # | Rule                                                   | Mechanism / why                                                                                                                                                                                           |
|---|--------------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 4 | **Endpoint mapping classes reside under `Features.*`** | Static stand-in for #3's placement intent. `Classes().That().HaveName("Endpoint").Should().ResideInNamespaceMatching(...)`.                                                                               |
| 5 | **Feature service registrations reside under `Features.*`** | Keeps slice-owned DI hooks in slices. `Classes().That().HaveName("FeatureServices").Should().ResideInNamespaceMatching(...)`.                                                                              |
| 6 | **Handlers are `sealed`**                              | Doc: "`Handler.cs` — a `sealed` `Handler` class". `Classes().That().HaveNameEndingWith("Handler").Should().BeSealed()`.                                                                                   |
| 7 | **Handlers use the per-slice name**                    | Reflection assertion requires the feature handler type name to be exactly `Handler`; a filesystem assertion requires slice-owned handler files to be named `Handler.cs`.                                  |
| 8 | **Handlers reside under `Features.*`**                 | Keeps business logic inside slices.                                                                                                                                                                       |

### Worth considering — 🟡 (expressible & useful, optional / tune later)

| #  | Rule                                                                           | Status                              | Why optional                                                                                                                                                                    |
|----|--------------------------------------------------------------------------------|-------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 9  | Handlers must not depend on ASP.NET HTTP/MVC plumbing                          | ✅ shipped, **tune the banned set** | Enforces "Endpoint = HTTP, Handler = logic". Currently bans `Microsoft.AspNetCore.Mvc` and `Microsoft.AspNetCore.Http`. Revisit if a handler legitimately needs e.g. `IResult`. |
| 10 | Entities in `Persistence.Entities`, EF configs in `Persistence.Configurations` | follow-up                           | Light placement guard; premature until Persistence exists, harmless when empty.                                                                                                 |
| 11 | Whole-API namespace cycle freedom                                              | ✅ shipped                          | `Slices().Matching("Bouw.API.(**)").Should().BeFreeOfCycles()` — complements #1/#2 at the layer level.                                                                          |
| 12 | Slice contracts live in `Contracts.cs` and remain in their slice               | follow-up                           | Placement is easy to assert if this convention starts drifting; low urgency while the folder shape is small.                                                                      |

### Would not — ❌ (skip, with reason)

| Candidate                                                       | Why not                                                                                                                 |
|-----------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------|
| Endpoint *runtime reachability* (#3)                            | Runtime endpoint data, not a static dep. Do it as a `WebApplicationFactory<Program>` integration test.                  |
| Ban MediatR / FluentValidation ("deliberately not used")        | Expressible, but better covered by *not referencing the package* + `BannedApiAnalyzers` (applied repo-wide). Redundant. |
| Contracts must be `record`s                                     | ArchUnitNET has no clean "is record" predicate; checking synthesized members is brittle, low value.                     |
| One type per file for slice contracts                           | Intentionally skipped. Slice-local request/response records may share `Contracts.cs`; keep one type per file for behavior-bearing code. |
| "Prefer duplication over the wrong abstraction" / rule-of-three | A human judgment call — not statically decidable.                                                                       |
| Slice-private helpers must be `internal`/`file`-scoped          | Accessibility is checkable, but the *intent* is fuzzy; not worth a brittle rule.                                        |
| File-length limits                                              | Already enforced by the `FileLineLimit` (LINE0001) analyzer, repo-wide.                                                 |

---

## ArchUnitNET 0.13.3 API notes (gotchas worth recording)

These were verified empirically against the package while wiring the rules:

- **`ResideInNamespace(x)` matches a namespace *exactly* (`^x$`)** — it does **not**
  include descendants, and there is **no** `(string, bool)` overload in 0.13.3.
  To match a namespace *and everything under it*, use
  **`ResideInNamespaceMatching(pattern)`** with an anchored-prefix regex, e.g.
  `^Bouw\.API\.Features(\.|$)`. The `(\.|$)` tail avoids matching a sibling like
  `FeaturesFoo`. All namespace patterns live as constants on `ArchitectureFixture`.
- **Slice matching is greedy.** In `Slices().Matching(...)`, both `(*)` and `(**)`
  capture the *entire* remaining namespace, so each distinct sub-namespace is its
  own slice. We use `(**)` to make "the slice is the full path after `Features.`"
  explicit.
- **Rules require positive results by default** — see the greenfield caveat above;
  hence `.WithoutRequiringPositiveResults()`.
- **net10 / Mono.Cecil:** ArchUnitNET 0.13.3 parses the net10.0 assembly,
  including collection expressions, without issue.

---

## Verifying the rules can fail (red-green)

`WithoutRequiringPositiveResults()` means an empty rule passes, so "all green" on a
greenfield repo proves little on its own. The rules were checked to *fail* on a
real violation by temporarily adding two throwaway slices under a shared grouping
folder:

- `Features/Workflows/CreateWorkflow/Handler.cs` (sealed)
- `Features/Workflows/EditWorkflow/Handler.cs` — **unsealed** and
  referencing the sibling slice's handler.

Result, as expected:

- **#1 failed** — the cross-slice reference was caught *even though both live under
  `Workflows/`*, confirming the grouping folder is not the boundary.
- **#6 failed** — the unsealed handler was caught.
- **#11 passed** — a one-way reference is not a cycle (no false positive).

The throwaway files were then deleted and the suite returned to green. Re-run this
check if you change the slice pattern or the namespace regexes.

---

## Out-of-scope follow-ups

- **#3 runtime reachability** — `WebApplicationFactory<Program>` integration test
  that asserts expected routes exist in the live `EndpointDataSource`.
- **#10 / #12** — Persistence and contract placement rules; add when those layers
  exist.
- **Harden vacuous rules** — once a layer is guaranteed non-empty, drop
  `WithoutRequiringPositiveResults()` for it.
