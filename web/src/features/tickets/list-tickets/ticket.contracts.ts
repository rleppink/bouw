import { z } from 'zod'

export const ticketStatusSchema = z.enum(['pending', 'completed'])
export type TicketStatus = z.infer<typeof ticketStatusSchema>

export const ticketSummarySchema = z.object({
  id: z.string(),
  title: z.string(),
  userInput: z.string(),
  status: ticketStatusSchema,
  llmResponse: z.string(),
  createdAt: z.string(),
  updatedAt: z.string(),
})
export type TicketSummary = z.infer<typeof ticketSummarySchema>

export const ticketListSchema = z.array(ticketSummarySchema)
