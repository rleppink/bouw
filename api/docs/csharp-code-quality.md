# C# Code Quality Setup

A reference for build-time code quality enforcement in a C# project (web/API, .NET 10+).

> This document was rewritten from a translation of a `golangci-lint` YAML config. The original carried two Go habits that don't fit C#: **"enable many small linters"** (C# doesn't have that ecosystem — the SDK ships a comprehensive pack and third parties overlap heavily), and **"lint is a separate step"** (in C#, lint *is* the build). The shape below curates rather than accumulates, and pushes a few concerns out of the build to where they fit better.

---

# Philosophy

In C#, **lint is the build**. The .NET SDK ships with `Microsoft.CodeAnalysis.NetAnalyzers` — several hundred rules maintained by Microsoft that move in lockstep with the framework. Roslyn analyzers from third parties plug into the same compile step. There is no separate linter binary, and no separate config file: analyzer severities live in `.editorconfig`, package references live in the csproj. The IDE surfaces every finding in real time as you type.

That changes the design space compared to ecosystems where lint is bolted on (Go's golangci-lint, JS's ESLint):

1. **Lean on the platform.** `AnalysisMode=All` activates the SDK's full ruleset. That alone covers most of what a multi-linter setup achieves elsewhere.
2. **Add a small number of curated extras**, not everything available. Analyzer rules overlap heavily under different IDs; piling on more packages adds noise without signal, and IDE distraction with it.
3. **Push some concerns out of the build.** Security taint analysis runs better as CodeQL out-of-band. Architecture rules feel more natural as test-time assertions. Vulnerability scanning is its own gate.
4. **Fail the build for every quality signal that matters.** `TreatWarningsAsErrors`, lock files, formatter verification. Warnings that don't fail the build are warnings that get ignored.

---

# The core stack

Every project that wants tight control should have this. Nothing in this section is optional.

## Build-time properties

`Directory.Build.props` at the solution root:

```xml
<Project>
  <PropertyGroup>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisLevel>10.0</AnalysisLevel>
    <AnalysisMode>All</AnalysisMode>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
    <NuGetAudit>true</NuGetAudit>
    <NuGetAuditMode>all</NuGetAuditMode>
    <NuGetAuditLevel>low</NuGetAuditLevel>
  </PropertyGroup>
</Project>
```

What each line does:

- `TreatWarningsAsErrors` — every analyzer finding fails the build. The single biggest discipline gain.
- `AnalysisLevel=10.0` + `AnalysisMode=All` — turns on the full SDK ruleset, pinned to a specific SDK level. Avoid `latest-all`: SDK upgrades shouldn't introduce new build failures by surprise. Bump the level deliberately.
- `EnforceCodeStyleInBuild` — promotes the `IDExxxx` style rules from IDE-only suggestions to build failures.
- `Nullable=enable` — null-related bugs become type errors.
- `RestorePackagesWithLockFile` — generates and uses `packages.lock.json`; restore fails when the lock file and the project drift.
- `NuGetAudit` + `NuGetAuditMode=all` + `NuGetAuditLevel=low` — restore fails on any known CVE in any direct or transitive dependency. Built into the SDK; replaces the older `dotnet list package --vulnerable --include-transitive` CLI step.

## Analyzer packages

A small, curated set in `Directory.Build.targets`:

```xml
<Project>
  <ItemGroup>
    <PackageReference Include="Microsoft.VisualStudio.Threading.Analyzers" PrivateAssets="all" />
    <PackageReference Include="Meziantou.Analyzer" PrivateAssets="all" />
    <PackageReference Include="Microsoft.CodeAnalysis.BannedApiAnalyzers" PrivateAssets="all" />
  </ItemGroup>
</Project>
```

Versions live in `Directory.Packages.props` (central package management). `PrivateAssets="all"` keeps analyzers out of downstream consumers.

Why these three:

- **`Microsoft.VisualStudio.Threading.Analyzers`** — async/await is the one area where the SDK's defaults leave gaps. Catches missing `ConfigureAwait(false)` in library code, sync-over-async (`.Result` / `.Wait()`), missing `CancellationToken` propagation, unsafe `async void`. Non-negotiable.
- **`Meziantou.Analyzer`** — opinionated quality and modernization rules with low overlap with the SDK pack. Pickier than Sonar without the false-positive volume. Pick this *or* Roslynator (see "Alternatives" below); don't add both.
- **`Microsoft.CodeAnalysis.BannedApiAnalyzers`** — declares a per-project `BannedSymbols.txt` listing APIs that must not be used: `DateTime.Now` (force `TimeProvider`), `Thread.Sleep` (force `Task.Delay`), `Task.Result` / `.Wait()` (force `await`), `new Random()` (force `Random.Shared` or `RandomNumberGenerator`). The build fails on usage.

That's it. The SDK pack carries the weight; these three fill specific gaps.

## What we deliberately do not add

These get debated. The recommendation is to skip them by default and add only if the listed reason applies.

- **`SonarAnalyzer.CSharp`** — overlaps ~80% with the CA rules under different IDs, and imports Java/SonarQube culture (numeric complexity gates as policy). **Add if** the team specifically wants hard cyclomatic / cognitive complexity ceilings (`S1541`, `S3776`, `S138`, `S134`, `S103`) and is willing to tune them in `.editorconfig`. Most C# teams don't gate on these numbers; they're a Go-culture import.
- **`StyleCop.Analyzers`** — most of its value collapses once CSharpier is in. What remains is naming/ordering (the IDE rules cover this) and documentation-comment policing that most app codebases don't want. **Add if** you specifically want enforced XML-doc comments on public APIs.
- **`SecurityCodeScan.VS2019`** — last release was 2021; effectively unmaintained, and its `VS2019` name shows. The CA5xxx family in the SDK pack covers crypto/RNG misuse. For taint analysis, use CodeQL out-of-band (see below). **Don't add.**
- **`Roslynator.Analyzers`** — fine alternative to Meziantou with similar coverage, somewhat less opinionated. Pick one or the other; the overlap with both active is wasted noise.
- **`AsyncFixer`, `ErrorProne.NET.CoreAnalyzers`** — niche pickups; the VS Threading analyzers already cover the high-value async cases. **Add if** you've identified a specific gap.

## Style enforcement

- **CSharpier** as a local dotnet tool, verified by `dotnet csharpier check .`. Opinionated, zero-config, handles whitespace, layout, and even csproj/props files deterministically.
- **`dotnet format --verify-no-changes`** verifies the `.editorconfig` style rules (the `IDExxxx` family).
- **`.editorconfig`** is the source of truth for analyzer severities, naming conventions, per-rule parameters (complexity thresholds if Sonar is added), and IDE preferences.

## Banned-symbol manifest

`BannedSymbols.txt` at the project root, registered via `<AdditionalFiles>` in `Directory.Build.props`. Bans symbols by `DocumentationCommentId`. Use it for:

- Time APIs (`DateTime.Now`, `DateTime.UtcNow`, `DateTime.Today`, `DateTimeOffset.Now/UtcNow`) — force `TimeProvider` for testability.
- Sync-blocking APIs (`Thread.Sleep`, `Task.Result`, `Task.Wait`).
- `new Random()` — force `Random.Shared` for non-security, `RandomNumberGenerator` for security.
- Any other library API the project decides is off-limits (e.g. `Process.Start` outside a designated subprocess project).

## File line-count limit

A hard 300-line-per-file ceiling, enforced as a **repo-local Roslyn analyzer** (`LINE0001`) rather than a CI script. This is the C#-native way to gate file size: it runs inline with every `dotnet build` — local CLI, IDE (live squiggles as a file crosses the limit), and CI alike — and fails the build via the existing `TreatWarningsAsErrors`. There is no off-the-shelf analyzer for this, but a custom one is ~60 lines.

Layout — a single-purpose sibling project in the same repo, referenced as an analyzer, **not** compiled into the app. (It holds one rule; if a real set of custom rules emerges later, give the project a set name and add rule classes to it then.)

```
analyzers/
└── FileLineLimit/
    ├── FileLineLimit.csproj                # netstandard2.0, references Microsoft.CodeAnalysis.CSharp
    └── FileLineCountAnalyzer.cs            # registers a SyntaxTreeAction, counts lines, reports LINE0001
```

Wiring:
- `Directory.Build.targets` references it for every project except itself:
  ```xml
  <ItemGroup Condition="'$(MSBuildProjectName)' != 'FileLineLimit'">
    <ProjectReference Include="$(MSBuildThisFileDirectory)analyzers/FileLineLimit/FileLineLimit.csproj"
                      OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
  </ItemGroup>
  ```
- The limit defaults to 300 and is overridable in `.editorconfig` via `max_file_lines`.
- The analyzer skips generated files (`ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None)`).
- Because the app project sits at the repo root, exclude the analyzer sources from its default compile glob: `<Compile Remove="analyzers/**/*.cs" />`.

This replaces the previous "shell script in CI" approach: a build-time analyzer is always enforced (you can't forget to run it) and surfaces in the editor, where a CI-only script never would.

---

# Out-of-band gates

Three concerns that fit poorly inside the build but are part of tight quality control.

## Security: CodeQL

For taint analysis (SQL injection, XSS, untrusted input reaching `Process.Start`, hardcoded credentials, insecure deserialization), use **GitHub Code Scanning with CodeQL** rather than a Roslyn-based security analyzer. CodeQL has a real, maintained query database and runs on every PR. SecurityCodeScan tried to do this inside the build and stopped shipping. On non-GitHub hosts, Snyk Code or Semgrep fill the same slot.

The CA5xxx family (crypto API misuse, weak hashing, insecure RNG) stays in the build via the SDK pack — that's the right layer for "are you using the crypto API correctly", as opposed to "does untrusted data flow to a sink".

## Dependencies: NuGet Audit + Dependabot

- `<NuGetAudit>true</NuGetAudit>` with `Mode=all` and `Level=low` fails restore on any known CVE in direct or transitive packages. Already wired in `Directory.Build.props` above.
- **Dependabot** (or Renovate) opens PRs for routine updates. Tight version control without manual `dotnet outdated` runs.

## Pre-commit: Husky.NET

Install **Husky.NET** to run `dotnet csharpier check` and `dotnet format --verify-no-changes` on staged files before commit. The "build is clean" invariant becomes real before push, not after CI fails. Strictly quality-of-life, but removes a common source of red CI builds.

---

# Test-time gates

Two concerns that belong with tests, not with analyzers.

## Architecture rules: ArchUnitNET

C# culture enforces architectural boundaries as assertions in the test suite:

```csharp
private static readonly Architecture Architecture = new ArchLoader()
    .LoadAssemblies(typeof(Domain.Marker).Assembly, typeof(Infrastructure.Marker).Assembly)
    .Build();

[Fact]
public void Domain_does_not_depend_on_EF_Core()
{
    IArchRule rule = Types()
        .That().ResideInNamespace("MyApp.Domain", useRegularExpressions: false)
        .Should().NotDependOnAny(
            Types().That().ResideInNamespace("Microsoft.EntityFrameworkCore"));
    rule.Check(Architecture);
}
```

This is more expressive than namespace-level rules (which is what NsDepCop and Go's `depguard` do). Use it for "Domain doesn't depend on Infrastructure", "no `Controller` references EF Core directly", "all `*Handler` classes implement `IHandler`", "no public type lives outside the `Api` namespace". The assertions live with the tests, run with the tests, and produce normal test failure output.

**ArchUnitNET** (`TngTech.ArchUnitNet`) is a port of the mature Java ArchUnit library and is actively maintained. It is still pre-1.0 (0.13.x as of early 2026) but releases regularly. Prefer it over **NetArchTest**, which has a simpler API but has been unmaintained since 2021.

**NsDepCop** remains a reasonable alternative if you specifically want namespace-level rules enforced at build time rather than test time. ArchUnitNET is the more capable, C#-native idiom; NsDepCop is the closer port of Go's `depguard`.

## Mutation testing: Stryker.NET

Coverage % tells you which lines executed. **Stryker.NET** tells you whether the tests actually assert anything: it mutates the production code (flips operators, removes statements, swaps return values) and re-runs the suite. A "killed" mutation means a test caught it; a "survived" mutation means a coverage gap or a weak assertion.

Run on changed code in CI with a survival threshold. This replaces the older "coverlet + ReportGenerator diff-coverage" gate as the headline quality metric — coverlet still runs underneath for raw coverage, but the *policy* gate is mutation score.

## Test framework analyzers

Once a test project exists, add the matching analyzer pack:

- **`xunit.analyzers`** for xUnit
- **`NUnit.Analyzers`** for NUnit

These catch async test pitfalls, missing `[Theory]` data, incorrect `[Fact]` signatures, and misused assertion APIs. Add at the point the test project is created.

---

# Local gate commands

No Makefile required; these are the canonical commands:

- `dotnet restore --locked-mode` — fails if `packages.lock.json` is stale or any package has a known CVE.
- `dotnet build` — succeeds only if every analyzer finding is resolved.
- `dotnet csharpier check .` — formatter verification.
- `dotnet format --verify-no-changes` — `.editorconfig` style verification.
- `dotnet test` — runs all tests, including NetArchTest assertions.
- `dotnet stryker` — mutation testing (slow; run on changed code).

Run all of these before pushing. Husky.NET handles the formatter step automatically on commit.

---

# Deferred until CI lands

The doc above covers local config only. When CI is added, these gates plug in:

- **Named, independently-runnable steps** — `check-format`, `check-build`, `check-restore`, `check-test`, `check-mutation`, `check-codeql` — so a failure points at the specific gate that broke.
- **`dotnet restore --locked-mode`** as the dedicated dependency-manifest gate.
- **Mutation testing on changed code** with a survival-rate threshold (e.g. ≥80% killed on lines changed in the PR).
- **CodeQL workflow** on PRs.
- **Ecosystem-specific analyzers**, added only when the corresponding tech is in use:
  - `Grpc.Tools` — protobuf diagnostics.
  - `OpenTelemetry.Instrumentation.*` — span / activity / metric hygiene.

---

# Summary: the shape

```
Build (Directory.Build.props):
  - SDK analyzers (CA + IDE rules)         [built in, AnalysisMode=All]
  - VS Threading Analyzers                  [+1 package]
  - Meziantou.Analyzer                      [+1 package]
  - BannedApiAnalyzers                      [+1 package]
  - FileLineCountAnalyzer (LINE0001)        [repo-local analyzer project]
  - Nullable, warnings-as-errors, lock files, NuGetAudit

Format:
  - CSharpier (+ Husky.NET pre-commit)
  - dotnet format / .editorconfig

Out-of-band:
  - CodeQL / GHAS for security taint analysis
  - Dependabot for dependency updates

Tests (when test project exists):
  - ArchUnitNET for architectural boundaries
  - Stryker.NET for mutation testing
  - xunit.analyzers / NUnit.Analyzers
```

Three third-party analyzer packages instead of ten. Same coverage where it matters; less noise; more time tuning the rules that fire on real bugs.
