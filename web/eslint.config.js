import js from '@eslint/js'
import pluginQuery from '@tanstack/eslint-plugin-query'
import pluginRouter from '@tanstack/eslint-plugin-router'
import jsxA11y from 'eslint-plugin-jsx-a11y'
import react from 'eslint-plugin-react'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import security from 'eslint-plugin-security'
import simpleImportSort from 'eslint-plugin-simple-import-sort'
import unicorn from 'eslint-plugin-unicorn'
import globals from 'globals'
import tseslint from 'typescript-eslint'
// eslint-config-prettier must be last — it turns off rules that conflict with
// the formatter so Prettier owns formatting and ESLint owns correctness.
import prettier from 'eslint-config-prettier'

export default tseslint.config(
  { ignores: ['dist/**', 'coverage/**', 'src/routeTree.gen.ts'] },

  // Everything gets the base JS rules.
  js.configs.recommended,

  // Type-aware + React rule set, scoped to application source so config files
  // (vite/vitest/eslint) don't need to live in a TS project.
  {
    files: ['src/**/*.{ts,tsx}'],
    extends: [
      // Significantly stronger than `recommended`: type-aware rules that catch
      // real bugs, not just style.
      ...tseslint.configs.strictTypeChecked,
      react.configs.flat.recommended,
      react.configs.flat['jsx-runtime'],
      jsxA11y.flatConfigs.recommended,
      // react-hooks v7 folds the former eslint-plugin-react-compiler rules into
      // `recommended-latest` (rules-of-hooks, exhaustive-deps, purity,
      // immutability, refs, set-state-in-effect, …) — the Rules of React, the
      // same surface the React Compiler relies on. The `flat.` namespace is the
      // flat-config build; the top-level key is the legacy eslintrc shape.
      reactHooks.configs.flat['recommended-latest'],
      ...pluginQuery.configs['flat/recommended'],
      ...pluginRouter.configs['flat/recommended'],
    ],
    languageOptions: {
      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
      globals: globals.browser,
    },
    settings: { react: { version: 'detect' } },
    plugins: {
      'react-refresh': reactRefresh,
      security,
      unicorn,
      'simple-import-sort': simpleImportSort,
    },
    rules: {
      // AI gets dependency arrays wrong — make it an error, not a warning.
      'react-hooks/exhaustive-deps': 'error',

      // Vite HMR: only export components from component files.
      'react-refresh/only-export-components': ['warn', { allowConstantExport: true }],

      // Deterministic import/export ordering.
      'simple-import-sort/imports': 'error',
      'simple-import-sort/exports': 'error',

      // eslint-plugin-security, curated: the full plugin is Node-oriented and
      // noisy in frontend code (e.g. detect-object-injection on every bracket
      // access). Only the two regex rules earn their keep here;
      // dangerouslySetInnerHTML is already covered by eslint-plugin-react.
      'security/detect-unsafe-regex': 'error',
      'security/detect-non-literal-regexp': 'error',

      // Source constraint dep-rules can't express: ky is wrapped once, in
      // @/lib/api. Everything else imports that wrapper.
      'no-restricted-imports': [
        'error',
        {
          paths: [
            {
              name: 'ky',
              message: 'Do not import ky directly — use the configured client in @/lib/api.',
            },
          ],
        },
      ],

      // eslint-plugin-unicorn, curated subset (NOT `recommended`).
      'unicorn/no-instanceof-array': 'error',
      'unicorn/throw-new-error': 'error',
      'unicorn/error-message': 'error',
      'unicorn/no-new-array': 'error',
      'unicorn/prefer-includes': 'error',
      'unicorn/no-for-loop': 'error',
      'unicorn/prefer-optional-catch-binding': 'error',
      'unicorn/prefer-logical-operator-over-ternary': 'error',
      'unicorn/prefer-string-trim-start-end': 'error',
      'unicorn/no-negated-condition': 'error',
      'unicorn/explicit-length-check': 'error',
      // …and the explicit disables (these fight React/idiomatic frontend code).
      'unicorn/prevent-abbreviations': 'off',
      'unicorn/no-null': 'off',
      'unicorn/no-array-reduce': 'off',
      'unicorn/prefer-ternary': 'off',
      'unicorn/consistent-destructuring': 'off',
      'unicorn/no-array-callback-reference': 'off',
    },
  },

  // Vendored shadcn/ui primitives: they export variant helpers alongside
  // components (breaking the fast-refresh rule by design) and are not our code
  // to restyle to the linter's taste. The Label primitive spreads `htmlFor`
  // from props, so the control association is made at the call site (where
  // jsx-a11y verifies it), not statically visible here.
  {
    files: ['src/components/ui/**'],
    rules: {
      'react-refresh/only-export-components': 'off',
      'jsx-a11y/label-has-associated-control': 'off',
    },
  },

  // TanStack Router file-based routes: each route module exports `Route`
  // (the result of createFileRoute, not a constant literal) alongside its
  // component. That's the prescribed pattern, and route-module HMR is handled
  // by the router plugin, so the fast-refresh component-only rule does not apply.
  {
    files: ['src/routes/**'],
    rules: {
      'react-refresh/only-export-components': 'off',
    },
  },

  // The one place ky may be imported.
  {
    files: ['src/lib/api.ts'],
    rules: { 'no-restricted-imports': 'off' },
  },

  // Tests assert against thrown/rejected values and use loose fixtures; relax
  // the strictest type-aware rules that fight test ergonomics.
  {
    files: ['src/**/*.test.{ts,tsx}', 'src/test/**'],
    rules: {
      '@typescript-eslint/no-non-null-assertion': 'off',
      '@typescript-eslint/no-unsafe-assignment': 'off',
    },
  },

  prettier,
)
