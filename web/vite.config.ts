import path from 'node:path'

import babel from '@rolldown/plugin-babel'
import tailwindcss from '@tailwindcss/vite'
import { tanstackRouter } from '@tanstack/router-plugin/vite'
import react, { reactCompilerPreset } from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    // The router plugin must run before the React plugin so the generated
    // route tree is transformed like any other source file.
    tanstackRouter({ target: 'react', autoCodeSplitting: true }),
    react(),
    // React Compiler runs as a Babel preset. plugin-react v6 (Vite 8 / Rolldown)
    // dropped its inline `babel` option, so the compiler is wired through
    // @rolldown/plugin-babel instead. Beyond the runtime optimisation, it
    // surfaces components it cannot compile as build signals — those components
    // violate the Rules of React.
    babel({ presets: [reactCompilerPreset()] }),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      '@': path.resolve(import.meta.dirname, './src'),
    },
  },
})
