import type { KnipConfig } from 'knip'

/**
 * Only the entry points knip can't infer are listed: TanStack Router's
 * file-based route modules and the vendored shadcn/ui primitives (deliberately
 * not orphan-checked). main.tsx, the config files, and the generated route tree
 * are already covered by knip's defaults.
 */
const config: KnipConfig = {
  entry: ['src/routes/**/*.tsx', 'src/components/ui/**/*.tsx'],
  project: ['src/**/*.{ts,tsx}'],
  ignoreDependencies: [
    // Imported from CSS (@import in src/index.css) — not from a module graph.
    'tailwindcss',
    'tw-animate-css',
    // shadcn/ui's configured icon library (components.json). No source imports
    // it yet, but it's kept for future generated components.
    'lucide-react',
  ],
}

export default config
