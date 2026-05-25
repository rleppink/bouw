import path from 'node:path'

import babel from '@rolldown/plugin-babel'
import tailwindcss from '@tailwindcss/vite'
import { tanstackRouter } from '@tanstack/router-plugin/vite'
import react, { reactCompilerPreset } from '@vitejs/plugin-react'
import { defineConfig, loadEnv, type UserConfig } from 'vite'

export function createViteConfig(mode: string): UserConfig {
  const env = loadEnv(mode, import.meta.dirname, '')
  const apiProxyTarget = env.API_PROXY_TARGET ?? 'http://localhost:5036'

  return {
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
    server: {
      // In Docker on macOS the bind mount doesn't forward fs events, so the
      // HMR watcher only sees changes when it polls. CHOKIDAR_USEPOLLING is
      // set by the compose `web` service.
      watch: {
        usePolling: env.CHOKIDAR_USEPOLLING === 'true',
      },
      proxy: {
        '/api': {
          target: apiProxyTarget,
          changeOrigin: true,
          rewrite: (requestPath) => requestPath.replace(/^\/api/, ''),
        },
      },
    },
  }
}

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  return createViteConfig(mode)
})
