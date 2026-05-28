import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'

import { useTicket } from './use-ticket'

type ViewTicketProps = {
  ticketId: string
}

export function ViewTicket({ ticketId }: ViewTicketProps) {
  const query = useTicket(ticketId)

  if (query.isPending) {
    return <p role="status">Loading ticket...</p>
  }

  if (query.isError) {
    return <p role="alert">Could not load ticket.</p>
  }

  if (!query.data) {
    return <p role="alert">Ticket not found.</p>
  }

  const ticket = query.data
  const isPending = ticket.status === 'pending'

  return (
    <section className="space-y-6">
      <header className="space-y-2">
        <div className="text-muted-foreground text-sm">{ticket.status}</div>
        <h1 className="text-2xl font-semibold">{ticket.title}</h1>
      </header>

      <div className="space-y-2">
        <Label htmlFor="ticket-user-input">User input</Label>
        <Textarea id="ticket-user-input" value={ticket.userInput} readOnly />
      </div>

      <div className="space-y-2">
        <Label htmlFor="ticket-llm-response">Response</Label>
        <Textarea
          id="ticket-llm-response"
          value={ticket.llmResponse}
          placeholder={isPending ? 'Response will appear here when ready.' : undefined}
          readOnly
        />
        {isPending ? (
          <p role="status" className="text-muted-foreground text-sm">
            Waiting for response...
          </p>
        ) : null}
      </div>
    </section>
  )
}
