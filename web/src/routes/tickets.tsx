import { createFileRoute, Link } from '@tanstack/react-router'
import { Plus } from 'lucide-react'

import { buttonVariants } from '@/components/ui/button'
import { ListTickets } from '@/features/tickets/list-tickets/list-tickets'

export const Route = createFileRoute('/tickets')({
  component: TicketsPage,
})

function TicketsPage() {
  return (
    <section className="space-y-3">
      <div className="flex items-center justify-between gap-3">
        <h1 className="text-xl font-semibold">Tickets</h1>
        <Link to="/tickets/new" className={buttonVariants()}>
          <Plus aria-hidden="true" className="size-4" />
          New ticket
        </Link>
      </div>
      <ListTickets />
    </section>
  )
}
