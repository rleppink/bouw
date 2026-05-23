# Frontend architecture tests (ArchUnitTS) — plan

The plan for enforcing the eventual frontend's architecture as executable tests,
mirroring what `api.tests/Architecture/` does for `Bouw.API`. This is a
**planning doc, not a wired-up suite** — there is no frontend in the repo yet
(no `package.json`, no Vite). It records the tool choice, how each backend
invariant ports, and the one caveat that behaves _opposite_ to the backend.

> **Scope.** [ArchUnitTS](https://github.com/LukasNiessen/ArchUnitTS) (`archunit`
> on npm) parses the TypeScript **source AST** and asserts properties of files,
> their import dependencies, and class-level metrics. Rules live in a test file
> and run as ordinary Vitest/Jest tests via an async matcher. It is the
> TS-ecosystem analogue of ArchUnitNET — fluent rules in the test suite, not a
> config file — which is why it's the choice here over dependency-cruiser.

---

## Decision: ArchUnitTS over the alternatives

| Tool                   | Why not (for us)                                                                                                                                                                                                                                                  |
| ---------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **dependency-cruiser** | Reasons over the _import graph_ only, via config. Excellent at dependency/cycle rules, but no class-level or metric rules, and it isn't tests-in-the-suite.                                                                                                       |
| **ts-arch**            | Closest fluent feel, but thinner: no code metrics, no custom rules, Jest-leaning, and no empty-test protection.                                                                                                                                                   |
| **ArchUnitTS** ✅      | Fluent rules in the test runner (matches the ArchUnitNET mental model), parses the AST (so class/metric rules are possible), framework-agnostic with a Vitest matcher, built-in code metrics, custom rules, and **empty-test protection** — see the caveat below. |

Version at time of writing: **v2.3.0** (May 2026), MIT, actively maintained.

---

## The mapping: backend invariant → ArchUnitTS

Legend — **✅ ports cleanly** · **🟡 ports, confirm exact API** · **❌ no equivalent**.
Rule numbers track `api/docs/archunit-tests.md` so the two suites stay legible
side by side.

| #   | Backend rule (ArchUnitNET)                                      | TS    | ArchUnitTS expression / note                                                                                                                                                                                                                                                                                                  |
| --- | --------------------------------------------------------------- | ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | No slice depends on another slice (THE invariant)               | 🟡    | Two routes. Declarative: `projectSlices().definedBy('features/(**)/').should().adhereToDiagram(diagram)` with a diagram that draws **no** edges between slices. Or per-pair `dependOnFiles` rules. **Confirm** whether a direct `notDependOnEachOther`-style slice rule exists; the diagram form is the documented mechanism. |
| 2   | Dependencies one-way: Features → Persistence/Infra, never back  | ✅    | `projectFiles().inFolder('features/**').shouldNot().dependOnFiles().inFolder('persistence/**')` (and the inverse direction asserted from the lower layer).                                                                                                                                                                    |
| 10  | Whole-app free of cycles                                        | ✅    | `projectFiles().inFolder('src/**').should().haveNoCycles()`.                                                                                                                                                                                                                                                                  |
| 8   | Handlers must not depend on HTTP/transport plumbing             | ✅    | `projectFiles().withName('*.handler.ts').shouldNot().dependOnFiles().inFolder(...)` — tune the banned set (e.g. the data-fetching client) as real handlers land, exactly as the backend's banned set is tuned.                                                                                                                |
| 5/7 | Endpoints/handlers reside inside a slice                        | ✅    | `projectFiles().withName(/Handler/).should().beInFolder('features/**')`. Largely subsumed by the folder structure + rule #2.                                                                                                                                                                                                  |
| 6   | Handlers are `sealed`                                           | ❌→🟡 | No `sealed` in TS. If an analogous structural guard is wanted, approximate with a custom rule (`.should().adhereTo((file) => …, desc)`), which has AST/content access. Likelier outcome: drop it — the frontend's "one obvious place per operation" is carried by structure, not a keyword.                                   |
| —   | File-length limit (backend's `FileLineLimit` LINE0001 analyzer) | ✅    | Built in: `metrics().count().linesOfCode().shouldBeBelow(N)`. The frontend gets the line-cap guardrail with no custom analyzer.                                                                                                                                                                                               |

Bonus surface ArchUnitTS adds that the backend has no analogue for, worth a look
once there's code: `metrics().lcom().lcom96b()` (cohesion),
`metrics().distance().distanceFromMainSequence()`, and Mermaid/DOT dependency
graph export for docs.

---

## The single most important caveat: empty-test protection is _inverted_

The backend is greenfield, so every type/class rule there carries
**`.WithoutRequiringPositiveResults()`** — a rule whose subject set is empty
**passes vacuously** (dormant guardrail) and only binds once matching types
appear. See `api/docs/archunit-tests.md` for the full reasoning.

ArchUnitTS does the **opposite by default**: if a pattern matches **zero files,
the test fails** (empty-test protection — it exists to catch a typo'd path that
would otherwise silently guard nothing). This is the _stricter, better_ default
once code exists, and it closes the exact hole the backend doc flags as the
trade-off of `WithoutRequiringPositiveResults()`.

The implication for a greenfield frontend is the mirror image of the backend
lifecycle:

- **While scaffolding (no slices yet):** opt out per rule —
  `await expect(rule).toPassAsync({ allowEmptyTests: true })` — so the dormant
  guardrails don't fail red before there's anything to guard.
- **As slices land:** drop `allowEmptyTests` from each rule the moment its target
  exists, regaining the "this rule is actually testing something" assurance —
  the same hardening the backend defers to "once a layer is guaranteed
  non-empty."

Net: same two-phase story as the backend (dormant → fails-closed), but the knob
is opt-_in_ relaxation rather than opt-_out_ strictness.

---

## Tooling notes (to verify when wiring up)

- `npm install --save-dev archunit`.
- Vitest needs `test.globals: true` for the matchers; the rule API is async —
  `await expect(rule).toPassAsync(opts)`. A framework-agnostic escape hatch
  exists: `const violations = await rule.check();`.
- Pattern matching supports globs (`*.handler.ts`, `features/**`) and regex
  (`/Handler\.ts$/`), with `{ except: { inPath: … } }` exclusions.
- HTML report + Mermaid/DOT/D2 graph export are available
  (`projectGraph().exportAsMermaid(...)`), and there's first-class Nx support
  (`nxProjectSlices()`) if the frontend ever becomes a monorepo workspace.

---

## Open questions / next steps

Resolve these before scaffolding the suite:

1. **Where the frontend lives.** A `web/` package in this repo (symmetric with
   `api/`), or a separate repo? This doc sits at the root "for now"; it should
   move to `web/docs/web-architecture-tests.md` once that folder exists, to
   parallel `api/docs/archunit-tests.md`.
2. **Folder/slice convention.** Does the frontend mirror the backend's vertical
   slices (one folder per operation under `features/`), or adopt a UI-shaped
   layering? The rule expressions above assume a `features/<slice>` shape; they
   change shape if the convention does.
3. **Red-green verification.** Port the backend's discipline (`archunit-tests.md`
   §"Verifying the rules can fail"): once the suite is wired, prove each rule
   fails on a deliberate violation before trusting an all-green run — doubly
   important here while `allowEmptyTests` is in play.
