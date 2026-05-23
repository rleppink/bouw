# web/ scaffold — remaining work

Status as of the latest pause. The blockers are resolved, the known type errors
are fixed, and the verification gauntlet is green. **All action points are now
complete** — `web/README.md` was written this session. No work remains; this
file is the record of how the scaffold landed.

## Verification gauntlet — status

| Gate                                | Status                                         |
| ----------------------------------- | ---------------------------------------------- |
| `npm run typecheck` (`tsc -b`)      | ✅ clean                                       |
| `npm run lint` (`eslint .`)         | ✅ clean (0 problems)                          |
| `npm run format:check`              | ✅ clean (whole tree formatted)                |
| `npm run test:cov`                  | ✅ 21/21 pass, branch **86.58%** (gate is 80%) |
| `npm run knip`                      | ✅ clean (lucide-react & tailwindcss ignored)  |
| `.size-limit.json` + `npm run size` | ✅ created; all 3 chunks within budget         |
| `npm run build`                     | ✅ clean (fresh build; React Compiler pass OK) |
| `web/README.md`                     | ✅ written; passes `prettier --check`          |

## Remaining action points

None. The scaffold is complete.

## Completed in this session

- **`web/README.md` written.** Covers the stack, prerequisites + getting
  started (incl. the `VITE_API_URL` default and `.env.local` opt-in), the
  project layout, every npm script, the quality gates (`npm run check` as the
  aggregate, plus `format:check` / `build` / `size` outside it, and the
  no-Husky/no-CI-by-decision note), and the architecture rules. Formatted to
  pass the project's `prettier --check` gate.

- **`.size-limit.json` created.** Three gzip-measured budgets — each entry sets
  `"gzip": true`, since `@size-limit/file` defaults to **brotli** and the
  baseline/budgets are stated in gzip. JS chunks at ~1.5× the fresh-build gzip
  baseline; CSS kept at the reference 12 kB for Tailwind utility-class headroom:
  - entry `dist/assets/index-*.js` — 93.5 kB → limit 145 kB
  - route chunk `dist/assets/workflows-*.js` — 54.9 kB → limit 85 kB
  - styles `dist/assets/*.css` — 4.0 kB → limit 12 kB
- **Fresh `npm run build` run and clean** (Vite 8.0.14, 582 modules transformed,
  React Compiler Babel pass ran without errors). Required for the baseline above,
  so the former "run build" action point is satisfied.
- **`npm run size` run** — all three chunks comfortably within budget, so the
  former "run size" action point is satisfied.

## Decisions made (previously blockers)

1. **React Compiler — Option A (build-time compiler).** Installed
   `@rolldown/plugin-babel`, `@babel/core`, `@types/babel__core`.
   `vite.config.ts` now wires the compiler as a Babel preset:
   `babel({ presets: [reactCompilerPreset()] })` (the v6 plugin-react dropped its
   inline `babel` option). `babel-plugin-react-compiler` stays, supplied by
   `reactCompilerPreset()`.

2. **Husky — skipped.** Git only supports one repo-wide `core.hooksPath`, so a
   hook can't be scoped to `web/` in this monorepo. Removed the `prepare` script,
   the `lint-staged` config block, and the `husky` + `lint-staged` devDeps. The
   quality gate is `npm run check` (typecheck + lint + knip + test:cov); `build`
   already runs `tsc -b`. No `web/.husky/` is created.

3. **CI workflow — not created.** Rely on the npm scripts. No `.github/` added.

## Fixes applied

- **`tsconfig.app.json`** — removed the deprecated `baseUrl`; under
  `moduleResolution: "bundler"` the `@/*` `paths` resolve relative to the
  tsconfig dir.
- **`vite.config.ts`** — replaced the old `react({ babel: { plugins } })` shape
  with the `@rolldown/plugin-babel` + `reactCompilerPreset()` wiring.
- **`src/lib/api.ts`** — ky 2 renamed `prefixUrl` → `baseUrl` (web-standard URL
  resolution; identical URLs for our leading-slash-free call sites).
- **`eslint.config.js`** — react-hooks v7 ships its flat config under
  `configs.flat['recommended-latest']`; the top-level `recommended-latest` key
  is the legacy eslintrc shape and crashes flat config. Added two override
  blocks: disable `jsx-a11y/label-has-associated-control` for the vendored
  `components/ui/**` (the Label primitive spreads `htmlFor`, so association is
  verified at the call site) and disable `react-refresh/only-export-components`
  for `routes/**` (each TanStack route module exports `Route` alongside its
  component by design).
- **`src/architecture.test.ts`** — two real problems fixed:
  - Pointed `projectFiles(...)` at `tsconfig.app.json` so the `@/*` alias
    resolves into real import-graph edges; without it the slice-independence
    rule could pass **vacuously**. Also raised the per-test timeout to 60s for
    the one-time ~20s graph build.
  - ArchUnitTS's `metrics().count().linesOfCode()` is **class-scoped** (it emits
    a subject only per `class` declaration), so it finds nothing in this
    class-free React codebase and cannot enforce a file cap. Replaced it with a
    custom filesystem rule (an eager `?raw` `import.meta.glob` over
    `src/**/*.{ts,tsx}`) mirroring the backend's LINE0001 semantics: 300-line
    cap, trailing-newline discount, generated `routeTree.gen.ts` exempt, plus an
    anti-vacuous guard asserting files were found.

## Notes / deviations from the brief

- **Two slices, not one.** `list-workflows` + `create-workflow`, so the "a slice
  never imports another slice" arch rule has something real to check. Slices
  share nothing directly; shared infra is `@/lib` and `@/components`,
  composition happens in `src/routes/workflows.tsx`.
- **ESLint pinned to 9** (not the create-vite default 10): `eslint-plugin-react`
  does not support ESLint 10 yet.
- **dependency-cruiser dropped**, ArchUnitTS only, matching
  `docs/web-architecture-tests.md`.
- **`eslint-plugin-tailwindcss` dropped** (Tailwind v4 support immature); class
  ordering is handled by `prettier-plugin-tailwindcss` instead.
- **`eslint-plugin-react-compiler` is gone upstream** — its rules are folded into
  `eslint-plugin-react-hooks` v7 (`flat['recommended-latest']`, enabled).
- Resolved bleeding-edge versions in play: Vite 8, TypeScript 6, Zod 4, ky 2,
  `@vitejs/plugin-react` 6 (Rolldown), archunit 2.3.
