import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { renderHook, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import type { Mock } from 'vitest'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { api } from '@/lib/api'
import { getGetTicketsUrl } from '@/lib/api.generated'

import { ticketsQueryKey, useTickets } from './use-tickets'

vi.mock('@/lib/api', () => ({
  api: {
    get: vi.fn(),
  },
}))

vi.mock('@/lib/api.generated', () => ({
  getGetTicketsUrl: vi.fn(),
}))

const mockGet = api.get as unknown as Mock
const mockGetTicketsUrl = getGetTicketsUrl as unknown as Mock

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })

  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
}

beforeEach(() => {
  vi.clearAllMocks()
  mockGetTicketsUrl.mockReturnValue('/tickets')
})

describe('useTickets', () => {
  it('loads tickets through the API client and parses the response', async () => {
    const tickets = [
      {
        id: '11111111-1111-1111-1111-111111111111',
        title: 'Build me a thing',
        userInput: 'Build me a thing',
        status: 'pending',
        llmResponse: '',
        createdAt: '2026-05-25T00:00:00Z',
        updatedAt: '2026-05-25T00:00:00Z',
      },
    ]
    mockGet.mockReturnValue({ json: vi.fn().mockResolvedValue(tickets) })

    const { result } = renderHook(() => useTickets(), { wrapper })

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true)
    })

    expect(mockGet).toHaveBeenCalledWith('tickets')
    expect(ticketsQueryKey).toEqual(['tickets'])
    expect(result.current.data).toEqual(tickets)
  })

  it('keeps generated paths without a leading slash unchanged', async () => {
    mockGetTicketsUrl.mockReturnValue('tickets')
    mockGet.mockReturnValue({ json: vi.fn().mockResolvedValue([]) })

    const { result } = renderHook(() => useTickets(), { wrapper })

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true)
    })

    expect(mockGet).toHaveBeenCalledWith('tickets')
  })

  it('surfaces invalid API data as a query error', async () => {
    mockGet.mockReturnValue({ json: vi.fn().mockResolvedValue([{ id: '1' }]) })

    const { result } = renderHook(() => useTickets(), { wrapper })

    await waitFor(() => {
      expect(result.current.isError).toBe(true)
    })
  })
})
