import { createEnv } from '@t3-oss/env-core'
import { z } from 'zod'

const apiUrlSchema = z
  .string()
  .refine((value) => URL.canParse(value) || value.startsWith('/'), {
    message: 'Expected an absolute URL or a root-relative path.',
  })

/**
 * Environment variables are external data, so the schema-first rule applies:
 * validate them once, at startup, and import `env` everywhere else. A missing
 * or malformed value fails loudly here rather than as `undefined` deep in a
 * request. Only `VITE_`-prefixed vars are exposed to the client by Vite.
 */
export const env = createEnv({
  clientPrefix: 'VITE_',
  client: {
    VITE_API_URL: apiUrlSchema.default('/api/'),
  },
  runtimeEnv: import.meta.env,
  emptyStringAsUndefined: true,
})
