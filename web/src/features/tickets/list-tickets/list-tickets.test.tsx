import { render, screen } from '@testing-library/react'
import type { ReactNode } from 'react'
import { describe, expect, it, vi } from 'vitest'

import { ListTickets } from './list-tickets'
import type { TicketSummary } from './ticket.contracts'
import { useTickets } from './use-tickets'

vi.mock('./use-tickets')
vi.mock('@tanstack/react-router', () => ({
  Link: ({
    children,
    className,
    params,
    to,
  }: {
    children: ReactNode
    className?: string
    params?: { id: string }
    to: string
  }) => (
    <a className={className} href={params ? to.replace('$id', params.id) : to}>
      {children}
    </a>
  ),
}))

const mockUseTickets = vi.mocked(useTickets)

function asResult(value: Partial<ReturnType<typeof useTickets>>): ReturnType<typeof useTickets> {
  return value as ReturnType<typeof useTickets>
}

const tickets: TicketSummary[] = [
  {
    id: '11111111-1111-1111-1111-111111111111',
    title: 'Build me a thing',
    userInput: 'Build me a thing',
    status: 'completed',
    llmResponse: 'gniht a em dliuB',
    createdAt: '2026-05-25T08:30:00Z',
    updatedAt: '2026-05-25T08:30:00Z',
  },
]

describe('ListTickets', () => {
  it('shows a loading state while pending', () => {
    mockUseTickets.mockReturnValue(asResult({ isPending: true, isError: false }))

    render(<ListTickets />)

    expect(screen.getByRole('status')).toHaveTextContent(/loading tickets/i)
  })

  it('shows an error state on failure', () => {
    mockUseTickets.mockReturnValue(asResult({ isPending: false, isError: true }))

    render(<ListTickets />)

    expect(screen.getByRole('alert')).toHaveTextContent(/could not load tickets/i)
  })

  it('shows an empty state', () => {
    mockUseTickets.mockReturnValue(asResult({ isPending: false, isError: false, data: [] }))

    render(<ListTickets />)

    expect(screen.getByText('No tickets yet.')).toBeInTheDocument()
  })

  it('renders tickets on success', () => {
    mockUseTickets.mockReturnValue(asResult({ isPending: false, isError: false, data: tickets }))

    render(<ListTickets />)

    expect(screen.getByRole('link', { name: /build me a thing/i })).toBeInTheDocument()
    expect(screen.getByText('completed')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /build me a thing/i })).toHaveAttribute(
      'href',
      '/tickets/11111111-1111-1111-1111-111111111111',
    )
  })
})
