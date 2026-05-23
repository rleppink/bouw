import { z } from 'zod'

/** This slice's own contracts — independent of list-workflows by design. */
export const createWorkflowInputSchema = z.object({
  name: z.string().min(3, 'Name must be at least 3 characters').max(80, 'Name is too long'),
  status: z.enum(['draft', 'active']),
})
export type CreateWorkflowInput = z.infer<typeof createWorkflowInputSchema>

export const createdWorkflowSchema = z.object({
  id: z.string(),
  name: z.string(),
})
export type CreatedWorkflow = z.infer<typeof createdWorkflowSchema>
