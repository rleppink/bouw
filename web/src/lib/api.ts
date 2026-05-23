import ky from 'ky'

import { env } from '@/env'

/**
 * The single configured HTTP client. Every feature talks to the backend
 * through this instance — direct `ky` imports are banned by ESLint
 * (`no-restricted-imports`) so base URL, retries, and timeouts live in exactly
 * one place.
 */
export const api = ky.create({
  // ky 2 renamed `prefixUrl` to `baseUrl` (web-standard URL resolution).
  // Relative inputs like `api.get('workflows')` resolve against this.
  baseUrl: env.VITE_API_URL,
  retry: { limit: 1 },
  timeout: 10_000,
})
