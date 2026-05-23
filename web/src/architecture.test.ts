import { projectFiles } from 'archunit'
import { describe, expect, it } from 'vitest'

/**
 * Executable architecture rules, mirroring `api.tests/Architecture/` for the
 * backend (see docs/web-architecture-tests.md). ArchUnitTS parses the TS source
 * AST and asserts properties of files and their import graph.
 *
 * Both entry points are pointed at tsconfig.app.json explicitly. That config
 * carries the `@/*` path alias and `include: ["src"]`, so ArchUnitTS resolves
 * `@/…` imports into real graph edges — without it the graph falls back to the
 * solution-style root tsconfig (no `paths`), and the slice-independence rule
 * would pass *vacuously* because no cross-slice edge could ever be seen.
 *
 * Note: ArchUnitTS fails a rule whose pattern matches zero files
 * (empty-test protection). Each rule below targets folders that already exist,
 * so none opt into `allowEmptyTests`; add it back only for a dormant guardrail
 * whose target slice has not landed yet.
 */
const TSCONFIG = 'tsconfig.app.json'

// Building the TypeScript program over the whole project is a one-time cost
// (~20s cold) that the first rule pays and the rest reuse; the default 5s
// per-test timeout is too tight for it.
const ARCH_TIMEOUT = 60_000

describe('architecture', () => {
  it(
    'a slice never depends on another slice (THE invariant)',
    async () => {
      const listOnCreate = projectFiles(TSCONFIG)
        .inFolder('src/features/workflows/list-workflows/**')
        .shouldNot()
        .dependOnFiles()
        .inFolder('src/features/workflows/create-workflow/**')

      const createOnList = projectFiles(TSCONFIG)
        .inFolder('src/features/workflows/create-workflow/**')
        .shouldNot()
        .dependOnFiles()
        .inFolder('src/features/workflows/list-workflows/**')

      await expect(listOnCreate).toPassAsync()
      await expect(createOnList).toPassAsync()
    },
    ARCH_TIMEOUT,
  )

  it(
    'shared UI components do not depend on features',
    async () => {
      const rule = projectFiles(TSCONFIG)
        .inFolder('src/components/**')
        .shouldNot()
        .dependOnFiles()
        .inFolder('src/features/**')

      await expect(rule).toPassAsync()
    },
    ARCH_TIMEOUT,
  )

  it(
    'shared lib does not depend on features or routes',
    async () => {
      const onFeatures = projectFiles(TSCONFIG)
        .inFolder('src/lib/**')
        .shouldNot()
        .dependOnFiles()
        .inFolder('src/features/**')

      const onRoutes = projectFiles(TSCONFIG)
        .inFolder('src/lib/**')
        .shouldNot()
        .dependOnFiles()
        .inFolder('src/routes/**')

      await expect(onFeatures).toPassAsync()
      await expect(onRoutes).toPassAsync()
    },
    ARCH_TIMEOUT,
  )

  it(
    'features do not depend on the composition layer (routes)',
    async () => {
      const rule = projectFiles(TSCONFIG)
        .inFolder('src/features/**')
        .shouldNot()
        .dependOnFiles()
        .inFolder('src/routes/**')

      await expect(rule).toPassAsync()
    },
    ARCH_TIMEOUT,
  )

  it(
    'the source is free of dependency cycles',
    async () => {
      const rule = projectFiles(TSCONFIG).inFolder('src/**').should().haveNoCycles()
      await expect(rule).toPassAsync()
    },
    ARCH_TIMEOUT,
  )

  // ArchUnitTS's metrics().count().linesOfCode() is class-scoped (it only emits
  // a subject per `class` declaration), so it finds nothing in this class-free
  // React codebase and cannot enforce a file cap. The backend gets this from a
  // custom analyzer (LINE0001); the honest mirror here is a custom filesystem
  // check with the same line-counting semantics.
  it('no source file exceeds the 300-line limit (mirrors backend LINE0001)', () => {
    const MAX_LINES = 300

    // Eager raw glob: Vite inlines every source file's text at transform time.
    const sources = import.meta.glob<string>('./**/*.{ts,tsx}', {
      query: '?raw',
      import: 'default',
      eager: true,
    })

    // Guard against a vacuous pass (ArchUnitTS's empty-test protection, by hand).
    expect(Object.keys(sources).length).toBeGreaterThan(0)

    const offenders = Object.entries(sources)
      // Generated code is exempt, matching the backend analyzer's
      // ConfigureGeneratedCodeAnalysis(None).
      .filter(([path]) => !path.endsWith('routeTree.gen.ts'))
      .map(([path, content]) => {
        const lines = content.split('\n')
        let count = lines.length
        // A trailing newline yields a final empty segment; the backend counts a
        // file of N content lines ending in "\n" as N, not N + 1.
        if (count > 0 && lines[count - 1] === '') count--
        return { path, count }
      })
      .filter(({ count }) => count > MAX_LINES)

    expect(offenders).toEqual([])
  })
})
