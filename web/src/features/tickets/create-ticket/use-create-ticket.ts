import { useMutation, useQueryClient } from '@tanstack/react-query'

import { api } from '@/lib/api'
import {
  type CreateTicketRequest,
  getCreateTicketUrl,
  type TicketResponse,
} from '@/lib/api.generated'

import { ticketsQueryKey } from '../list-tickets/use-tickets'

function toKyPath(path: string): string {
  return path.startsWith('/') ? path.slice(1) : path
}

async function createTicket(request: CreateTicketRequest): Promise<TicketResponse> {
  return api.post(toKyPath(getCreateTicketUrl()), { json: request }).json<TicketResponse>()
}

export function useCreateTicket() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createTicket,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ticketsQueryKey })
    },
  })
}
