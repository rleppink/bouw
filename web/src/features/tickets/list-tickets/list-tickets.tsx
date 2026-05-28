import { Link } from '@tanstack/react-router'

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

import { useTickets } from './use-tickets'

export function ListTickets() {
  const query = useTickets()

  if (query.isPending) {
    return <p role="status">Loading tickets...</p>
  }

  if (query.isError) {
    return <p role="alert">Could not load tickets.</p>
  }

  return (
    <section className="space-y-4">
      {query.data.length === 0 ? (
        <p className="text-muted-foreground text-sm">No tickets yet.</p>
      ) : (
        <ul className="space-y-2">
          {query.data.map((ticket) => (
            <li key={ticket.id}>
              <Link
                to="/tickets/$id"
                params={{ id: ticket.id }}
                className="block"
              >
                <Card className="hover:bg-muted/50 transition-colors">
                  <CardHeader>
                    <CardTitle>{ticket.title}</CardTitle>
                    <p className="text-muted-foreground text-sm">{ticket.status}</p>
                  </CardHeader>
                  <CardContent className="text-muted-foreground line-clamp-2 text-sm">
                    {ticket.userInput}
                  </CardContent>
                </Card>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
