import { createFileRoute } from '@tanstack/react-router'

import { ViewTicket } from '@/features/tickets/view-ticket/view-ticket'

export const Route = createFileRoute('/tickets_/$id')({
  component: TicketPage,
})

function TicketPage() {
  const { id } = Route.useParams()
  return <ViewTicket ticketId={id} />
}
