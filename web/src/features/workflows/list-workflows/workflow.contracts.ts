import { z } from 'zod'

/**
 * What this slice needs from the API — defined here, not shared. A different
 * slice that also touches workflows declares its own shape (see
 * create-workflow). Duplication is preferred over a shared contract that would
 * couple the slices.
 */
export const workflowStatusSchema = z.enum(['draft', 'active', 'archived'])
export type WorkflowStatus = z.infer<typeof workflowStatusSchema>

export const workflowSummarySchema = z.object({
  id: z.string(),
  name: z.string(),
  status: workflowStatusSchema,
  createdAt: z.string(),
})
export type WorkflowSummary = z.infer<typeof workflowSummarySchema>

export const workflowListSchema = z.array(workflowSummarySchema)
