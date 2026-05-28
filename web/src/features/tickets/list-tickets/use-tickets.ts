import { useQuery } from '@tanstack/react-query'

import { api } from '@/lib/api'
import { getGetTicketsUrl } from '@/lib/api.generated'

import { ticketListSchema, type TicketSummary } from './ticket.contracts'

export const ticketsQueryKey = ['tickets'] as const

function toKyPath(path: string): string {
  return path.startsWith('/') ? path.slice(1) : path
}

async function fetchTickets(): Promise<TicketSummary[]> {
  const data = await api.get(toKyPath(getGetTicketsUrl())).json()
  return ticketListSchema.parse(data)
}

export function useTickets() {
  return useQuery({
    queryKey: ticketsQueryKey,
    queryFn: fetchTickets,
  })
}
