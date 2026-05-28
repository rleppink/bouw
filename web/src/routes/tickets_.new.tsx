import { createFileRoute } from '@tanstack/react-router'

import { CreateTicket } from '@/features/tickets/create-ticket/create-ticket'

export const Route = createFileRoute('/tickets_/new')({
  component: NewTicketPage,
})

function NewTicketPage() {
  return (
    <section className="space-y-3">
      <h1 className="text-xl font-semibold">New ticket</h1>
      <CreateTicket />
    </section>
  )
}
