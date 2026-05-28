import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { renderHook, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import type { Mock } from 'vitest'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { api } from '@/lib/api'
import { getGetTicketUrl, type TicketResponse } from '@/lib/api.generated'

import { ticketQueryKey, useTicket } from './use-ticket'

vi.mock('@/lib/api', () => ({
  api: {
    get: vi.fn(),
  },
}))

vi.mock('@/lib/api.generated', () => ({
  getGetTicketUrl: vi.fn(),
}))

const mockGet = api.get as unknown as Mock
const mockGetTicketUrl = getGetTicketUrl as unknown as Mock

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })

  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
}

const ticket: TicketResponse = {
  id: '11111111-1111-1111-1111-111111111111',
  title: 'Build me a thing',
  userInput: 'Build me a thing',
  status: 'completed',
  llmResponse: 'Done.',
  createdAt: '2026-05-25T00:00:00+00:00',
  updatedAt: '2026-05-25T00:00:00+00:00',
}

beforeEach(() => {
  vi.clearAllMocks()
  mockGetTicketUrl.mockReturnValue('/tickets/11111111-1111-1111-1111-111111111111')
})

describe('useTicket', () => {
  it('loads a ticket using the generated API path', async () => {
    mockGet.mockResolvedValue({
      ok: true,
      status: 200,
      json: vi.fn().mockResolvedValue(ticket),
    })

    const { result } = renderHook(() => useTicket('11111111-1111-1111-1111-111111111111'), {
      wrapper,
    })

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true)
    })

    expect(mockGetTicketUrl).toHaveBeenCalledWith('11111111-1111-1111-1111-111111111111')
    expect(mockGet).toHaveBeenCalledWith('tickets/11111111-1111-1111-1111-111111111111', {
      throwHttpErrors: false,
    })
    expect(ticketQueryKey('11111111-1111-1111-1111-111111111111')).toEqual([
      'ticket',
      '11111111-1111-1111-1111-111111111111',
    ])
    expect(result.current.data).toEqual(ticket)
  })

  it('keeps generated paths without a leading slash unchanged', async () => {
    mockGetTicketUrl.mockReturnValue('tickets/11111111-1111-1111-1111-111111111111')
    mockGet.mockResolvedValue({
      ok: true,
      status: 200,
      json: vi.fn().mockResolvedValue(ticket),
    })

    const { result } = renderHook(() => useTicket('11111111-1111-1111-1111-111111111111'), {
      wrapper,
    })

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true)
    })

    expect(mockGet).toHaveBeenCalledWith('tickets/11111111-1111-1111-1111-111111111111', {
      throwHttpErrors: false,
    })
  })

  it('returns null when the ticket is not found', async () => {
    mockGet.mockResolvedValue({
      ok: false,
      status: 404,
      json: vi.fn(),
    })

    const { result } = renderHook(() => useTicket('missing'), { wrapper })

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true)
    })

    expect(result.current.data).toBeNull()
  })

  it('throws for non-404 API failures', async () => {
    mockGet.mockResolvedValue({
      ok: false,
      status: 500,
      json: vi.fn(),
    })

    const { result } = renderHook(() => useTicket('11111111-1111-1111-1111-111111111111'), {
      wrapper,
    })

    await waitFor(() => {
      expect(result.current.isError).toBe(true)
    })
    expect(result.current.error).toEqual(
      new Error("Could not load ticket '11111111-1111-1111-1111-111111111111'."),
    )
  })
})
