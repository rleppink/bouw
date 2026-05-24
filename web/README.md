# web

The `bouw` frontend: a vertical-slice React app that talks to the `Bouw.API`
backend. Each user-facing operation lives in its own slice under
`src/features/`; slices never import one another, and routes are the only
composition layer. The same architectural discipline as the backend, enforced
as executable tests (see [Architecture](#architecture)).

## Prerequisites

- **Node** `^20.19.0 || >=22.12.0` (Vite 8's engine requirement).
- The backend running locally on `http://localhost:5036` if you want live data;
  Vite proxies `/api` to it during development.

## Getting started

```bash
npm install
npm run dev       # Vite dev server on http://localhost:5173
```

No `.env` file is required for local development. `VITE_API_URL` defaults to
`/api/` (validated at startup in `src/env.ts`), and Vite proxies that path to
`http://localhost:5036`. To point the proxy at a different backend, create
`web/.env.local`:

```bash
API_PROXY_TARGET=http://localhost:8080
```

To bypass the dev proxy entirely and call an API URL directly, set:

```bash
VITE_API_URL=https://api.example.com
```

Only `VITE_`-prefixed variables are exposed to the client.

## Stack

| Concern            | Choice                                                             |
| ------------------ | ------------------------------------------------------------------ |
| Build / dev server | **Vite 8** (Rolldown) + `@vitejs/plugin-react` 6                   |
| Language           | **TypeScript 6** (strict, `@/*` path alias → `src/`)               |
| UI                 | **React 19** with the **React Compiler** (wired as a Babel preset) |
| Routing            | **TanStack Router** — file-based routes, auto code-splitting       |
| Server state       | **TanStack Query** (the `api` client is **ky 2**)                  |
| Client state       | **Zustand**                                                        |
| Forms + validation | **React Hook Form** + **Zod 4** (via `@hookform/resolvers`)        |
| Styling            | **Tailwind CSS v4** (`@tailwindcss/vite`); shadcn-style primitives |
| Dates              | **date-fns**                                                       |

The React Compiler does more than optimise runtime: components it cannot compile
surface as **build signals**, flagging Rules-of-React violations.

## Project layout

```
src/
  features/<domain>/<slice>/   vertical slices — never import each other
    list-workflows/
    create-workflow/
  routes/                      the composition layer (may use multiple slices)
  components/ui/               vendored shadcn-style primitives
  lib/                         shared infra: api (ky), query-client, utils
  env.ts                       validated environment (Zod)
  architecture.test.ts         the architecture rules (ArchUnitTS + a line cap)
```

## npm scripts

| Script                 | What it does                                            |
| ---------------------- | ------------------------------------------------------- |
| `npm run dev`          | Start the Vite dev server (HMR).                        |
| `npm run build`        | `tsc -b` then `vite build`. Type-checks, then bundles.  |
| `npm run preview`      | Serve the production build locally.                     |
| `npm run typecheck`    | `tsc -b` — types only, no emit.                         |
| `npm run lint`         | `eslint .`                                              |
| `npm run format`       | `prettier --write .`                                    |
| `npm run format:check` | `prettier --check .` (no writes; the CI-style check).   |
| `npm run test`         | Run the test suite once (Vitest).                       |
| `npm run test:watch`   | Vitest in watch mode.                                   |
| `npm run test:cov`     | Tests with V8 coverage (branch gate: **80%**).          |
| `npm run knip`         | Find unused files, exports, and dependencies.           |
| `npm run size`         | Check bundle budgets against `.size-limit.json` (gzip). |
| `npm run check`        | The aggregate gate — see below.                         |

## Quality gates

`npm run check` is the **aggregate gate**:

```
npm run check   # typecheck → lint → knip → test:cov
```

Run it before pushing. Three checks live outside it because they need writes or
build artifacts:

```bash
npm run format:check   # formatting (run `npm run format` to fix)
npm run build          # also runs `tsc -b`; produces dist/ for size
npm run size           # bundle budgets, measured against the fresh dist/
```

There is **no Husky and no CI by decision.** Git's repo-wide `core.hooksPath`
can't be scoped to `web/` inside this monorepo, so the gate is run **manually**.
`npm run build` runs `tsc -b`, so a build can never ship type errors.

## Architecture

The architecture is enforced as tests in `src/architecture.test.ts`
(via [ArchUnitTS](https://github.com/LukasNiessen/ArchUnitTS)), mirroring the
backend's `api.tests/Architecture/`. The rules:

- **No slice imports another slice** — the core invariant. Slices share only
  `@/lib` and `@/components`; composition happens in `src/routes/`.
- **No import cycles** anywhere in `src/`.
- **A 300-line file cap** (a custom filesystem rule; `routeTree.gen.ts` is
  exempt as generated).

See [`docs/web-architecture-tests.md`](docs/web-architecture-tests.md) for the
full rationale and the backend-invariant mapping.
