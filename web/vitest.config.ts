import { defineConfig, mergeConfig } from 'vitest/config'

import { createViteConfig } from './vite.config'

// Inherits the path alias from vite.config so `@/…` resolves in tests too.
export default mergeConfig(
  createViteConfig('test'),
  defineConfig({
    test: {
      globals: true,
      environment: 'jsdom',
      setupFiles: ['./src/test/setup.ts'],
      css: true,
      coverage: {
        provider: 'v8',
        reporter: ['text', 'html'],
        // Coverage is measured over feature logic — the place AI skips the hard
        // conditional cases. Declarative contracts, generated code, and the
        // composition layer (routes/main) carry no branches worth gating.
        include: ['src/features/**'],
        exclude: ['**/*.test.{ts,tsx}', '**/*.contracts.ts', '**/index.ts'],
        // Branch coverage catches untested conditionals; line coverage is easy
        // to game. 80% branch minimum is a hard gate (see web README).
        thresholds: { branches: 80 },
      },
    },
  }),
)
