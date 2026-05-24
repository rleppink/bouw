import { z } from 'zod'

/**
 * What this slice needs from the API — defined here, not shared. A different
 * slice that also touches workflows declares its own shape. Duplication is
 * preferred over a shared contract that would couple the slices.
 */
export const workflowStatusSchema = z.enum(['draft', 'active', 'archived'])
export type WorkflowStatus = z.infer<typeof workflowStatusSchema>

export const workflowSummarySchema = z.object({
  id: z.string(),
  name: z.string(),
  description: z.string(),
  status: workflowStatusSchema,
  steps: z.array(z.unknown()),
})
export type WorkflowSummary = z.infer<typeof workflowSummarySchema>

export const workflowListSchema = z.array(workflowSummarySchema)
