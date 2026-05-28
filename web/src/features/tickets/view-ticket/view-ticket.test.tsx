import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'

import type { TicketResponse } from '@/lib/api.generated'

import { useTicket } from './use-ticket'
import { ViewTicket } from './view-ticket'

vi.mock('./use-ticket')

const mockUseTicket = vi.mocked(useTicket)

function asResult(value: Partial<ReturnType<typeof useTicket>>): ReturnType<typeof useTicket> {
  return value as ReturnType<typeof useTicket>
}

const ticket: TicketResponse = {
  id: '11111111-1111-1111-1111-111111111111',
  title: 'Build me a thing',
  userInput: 'Build me a thing',
  status: 'completed',
  llmResponse: 'gniht a em dliuB',
  createdAt: '2026-05-25T08:30:00Z',
  updatedAt: '2026-05-25T08:30:00Z',
}

describe('ViewTicket', () => {
  it('shows a loading state while pending', () => {
    mockUseTicket.mockReturnValue(asResult({ isPending: true, isError: false }))

    render(<ViewTicket ticketId="11111111-1111-1111-1111-111111111111" />)

    expect(screen.getByRole('status')).toHaveTextContent(/loading ticket/i)
  })

  it('shows an error state on failure', () => {
    mockUseTicket.mockReturnValue(asResult({ isPending: false, isError: true }))

    render(<ViewTicket ticketId="11111111-1111-1111-1111-111111111111" />)

    expect(screen.getByRole('alert')).toHaveTextContent(/could not load ticket/i)
  })

  it('shows a not found state when the API returns no ticket', () => {
    mockUseTicket.mockReturnValue(asResult({ isPending: false, isError: false, data: null }))

    render(<ViewTicket ticketId="missing" />)

    expect(screen.getByRole('alert')).toHaveTextContent(/not found/i)
  })

  it('renders user input and read-only response', () => {
    mockUseTicket.mockReturnValue(asResult({ isPending: false, isError: false, data: ticket }))

    render(<ViewTicket ticketId="11111111-1111-1111-1111-111111111111" />)

    expect(screen.getByRole('heading', { name: 'Build me a thing' })).toBeInTheDocument()
    expect(screen.getByLabelText('User input')).toHaveValue('Build me a thing')
    expect(screen.getByLabelText('Response')).toHaveValue('gniht a em dliuB')
    expect(screen.getByLabelText('Response')).toHaveAttribute('readonly')
  })

  it('renders a waiting state while the response is pending', () => {
    mockUseTicket.mockReturnValue(
      asResult({
        isPending: false,
        isError: false,
        data: { ...ticket, status: 'pending', llmResponse: '' },
      }),
    )

    render(<ViewTicket ticketId="11111111-1111-1111-1111-111111111111" />)

    expect(screen.getByRole('status')).toHaveTextContent(/waiting for response/i)
    expect(screen.getByLabelText('Response')).toHaveValue('')
  })
})
