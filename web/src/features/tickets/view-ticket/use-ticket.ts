import { useQuery } from '@tanstack/react-query'

import { api } from '@/lib/api'
import { getGetTicketUrl, type TicketResponse } from '@/lib/api.generated'

export const ticketQueryKey = (id: string) => ['ticket', id] as const

function toKyPath(path: string): string {
  return path.startsWith('/') ? path.slice(1) : path
}

async function fetchTicket(id: string): Promise<TicketResponse | null> {
  const response = await api.get(toKyPath(getGetTicketUrl(id)), {
    throwHttpErrors: false,
  })

  if (response.status === 404) {
    return null
  }

  if (!response.ok) {
    throw new Error(`Could not load ticket '${id}'.`)
  }

  return response.json<TicketResponse>()
}

export function useTicket(id: string) {
  return useQuery({
    queryKey: ticketQueryKey(id),
    queryFn: () => fetchTicket(id),
    refetchInterval: (query) => (query.state.data?.status === 'pending' ? 1000 : false),
  })
}
