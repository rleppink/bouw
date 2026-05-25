import { GripVertical, RotateCcw } from 'lucide-react'
import { type DragEvent, useMemo, useState } from 'react'

import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { cn } from '@/lib/utils'

import { initialTickets, lanes, type Ticket } from './board-data'

type DropTarget = {
  laneId: string
  beforeTicketId?: string
}

export function Board() {
  const [tickets, setTickets] = useState(initialTickets)
  const [draggedTicketId, setDraggedTicketId] = useState<string | null>(null)
  const [dropTarget, setDropTarget] = useState<DropTarget | null>(null)
  const [previewPosition, setPreviewPosition] = useState<{ x: number; y: number } | null>(null)

  const draggedTicket = useMemo(
    () => tickets.find((ticket) => ticket.id === draggedTicketId),
    [draggedTicketId, tickets],
  )

  function moveTicket(ticketId: string, target: DropTarget) {
    setTickets((currentTickets) => {
      const ticket = currentTickets.find((item) => item.id === ticketId)
      if (!ticket) return currentTickets

      const remainingTickets = currentTickets.filter((item) => item.id !== ticketId)
      const movedTicket = { ...ticket, laneId: target.laneId }
      const beforeIndex = target.beforeTicketId
        ? remainingTickets.findIndex((item) => item.id === target.beforeTicketId)
        : -1

      if (beforeIndex === -1) {
        return [...remainingTickets, movedTicket]
      }

      return [
        ...remainingTickets.slice(0, beforeIndex),
        movedTicket,
        ...remainingTickets.slice(beforeIndex),
      ]
    })
  }

  function startDrag(event: DragEvent<HTMLDivElement>, ticketId: string) {
    event.dataTransfer.effectAllowed = 'move'
    event.dataTransfer.setData('text/plain', ticketId)
    setDraggedTicketId(ticketId)
    setPreviewPosition({ x: event.clientX, y: event.clientY })
  }

  function trackDrag(event: DragEvent) {
    if (event.clientX === 0 && event.clientY === 0) return
    setPreviewPosition({ x: event.clientX, y: event.clientY })
  }

  function allowDrop(event: DragEvent, target: DropTarget) {
    event.preventDefault()
    event.dataTransfer.dropEffect = 'move'
    setDropTarget(target)
    trackDrag(event)
  }

  function dropTicket(event: DragEvent, target: DropTarget) {
    event.preventDefault()
    const ticketId = event.dataTransfer.getData('text/plain') || draggedTicketId
    if (ticketId) {
      moveTicket(ticketId, target)
    }
    endDrag()
  }

  function endDrag() {
    setDraggedTicketId(null)
    setDropTarget(null)
    setPreviewPosition(null)
  }

  return (
    <section className="space-y-4">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="space-y-1">
          <h1 className="text-xl font-semibold">Delivery board</h1>
          <p className="text-muted-foreground text-sm">
            Drag tickets between lanes to reshape the current sprint.
          </p>
        </div>
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => {
            setTickets(initialTickets)
          }}
        >
          <RotateCcw className="size-4" aria-hidden="true" />
          Reset
        </Button>
      </div>

      <div className="grid gap-3 overflow-x-auto pb-2 md:grid-cols-5">
        {lanes.map((lane) => {
          const laneTickets = tickets.filter((ticket) => ticket.laneId === lane.id)
          const isLaneTarget =
            dropTarget?.laneId === lane.id && dropTarget.beforeTicketId === undefined

          return (
            <section
              key={lane.id}
              aria-labelledby={`${lane.id}-heading`}
              onDragOver={(event) => {
                allowDrop(event, { laneId: lane.id })
              }}
              onDrop={(event) => {
                dropTicket(event, { laneId: lane.id })
              }}
              className={cn(
                'bg-muted/45 min-h-96 rounded-lg border p-2 transition-colors',
                isLaneTarget && 'border-primary bg-accent',
              )}
            >
              <header className="mb-2 flex items-center justify-between gap-2 px-1">
                <div>
                  <h2 id={`${lane.id}-heading`} className="text-sm font-semibold">
                    {lane.title}
                  </h2>
                  <p className="text-muted-foreground text-xs">
                    {laneTickets.length} / {lane.limit}
                  </p>
                </div>
              </header>

              <div className="space-y-2">
                {laneTickets.map((ticket) => (
                  <TicketCard
                    key={ticket.id}
                    ticket={ticket}
                    isDragging={ticket.id === draggedTicketId}
                    isDropTarget={dropTarget?.beforeTicketId === ticket.id}
                    onDragStart={(event) => {
                      startDrag(event, ticket.id)
                    }}
                    onDrag={(event) => {
                      trackDrag(event)
                    }}
                    onDragEnd={endDrag}
                    onDragOver={(event) => {
                      allowDrop(event, { laneId: lane.id, beforeTicketId: ticket.id })
                    }}
                    onDrop={(event) => {
                      dropTicket(event, { laneId: lane.id, beforeTicketId: ticket.id })
                    }}
                  />
                ))}
              </div>
            </section>
          )
        })}
      </div>

      {draggedTicket && previewPosition ? (
        <div
          aria-hidden="true"
          className="pointer-events-none fixed z-50 w-64 -translate-x-4 -translate-y-4 opacity-95 shadow-2xl"
          style={{ left: previewPosition.x, top: previewPosition.y }}
        >
          <TicketShell ticket={draggedTicket} isPreview />
        </div>
      ) : null}
    </section>
  )
}

type TicketCardProps = {
  ticket: Ticket
  isDragging: boolean
  isDropTarget: boolean
  onDragStart: (event: DragEvent<HTMLDivElement>) => void
  onDrag: (event: DragEvent<HTMLDivElement>) => void
  onDragEnd: () => void
  onDragOver: (event: DragEvent<HTMLDivElement>) => void
  onDrop: (event: DragEvent<HTMLDivElement>) => void
}

function TicketCard({
  ticket,
  isDragging,
  isDropTarget,
  onDragStart,
  onDrag,
  onDragEnd,
  onDragOver,
  onDrop,
}: TicketCardProps) {
  return (
    <div
      draggable
      onDragStart={onDragStart}
      onDrag={onDrag}
      onDragEnd={onDragEnd}
      onDragOver={onDragOver}
      onDrop={onDrop}
      className={cn(
        'rounded-xl transition-transform outline-none',
        isDragging && 'scale-95 opacity-35',
        isDropTarget && 'ring-primary ring-2 ring-offset-2',
      )}
    >
      <TicketShell ticket={ticket} />
    </div>
  )
}

function TicketShell({ ticket, isPreview = false }: { ticket: Ticket; isPreview?: boolean }) {
  return (
    <Card
      className={cn(
        'cursor-grab rounded-lg shadow-sm active:cursor-grabbing',
        isPreview && 'rotate-1',
      )}
    >
      <CardHeader className="flex-row items-start gap-2 p-3 pb-2">
        <GripVertical className="text-muted-foreground mt-0.5 size-4 shrink-0" aria-hidden="true" />
        <div className="min-w-0 space-y-1">
          <CardTitle className="text-sm leading-snug">{ticket.title}</CardTitle>
          <p className="text-muted-foreground text-xs">{ticket.id}</p>
        </div>
      </CardHeader>
      <CardContent className="flex items-center justify-between gap-2 p-3 pt-0 text-xs">
        <span>{ticket.owner}</span>
        <span className="text-muted-foreground">
          {ticket.priority} · {ticket.estimate} pt
        </span>
      </CardContent>
    </Card>
  )
}
