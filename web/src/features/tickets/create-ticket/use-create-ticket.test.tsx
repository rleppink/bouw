import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { renderHook, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import type { Mock } from 'vitest'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { api } from '@/lib/api'
import { getCreateTicketUrl, type TicketResponse } from '@/lib/api.generated'

import { useCreateTicket } from './use-create-ticket'

vi.mock('@/lib/api', () => ({
  api: {
    post: vi.fn(),
  },
}))

vi.mock('@/lib/api.generated', () => ({
  getCreateTicketUrl: vi.fn(),
}))

const mockPost = api.post as unknown as Mock
const mockGetCreateTicketUrl = getCreateTicketUrl as unknown as Mock

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
  status: 'pending',
  llmResponse: '',
  createdAt: '2026-05-25T00:00:00+00:00',
  updatedAt: '2026-05-25T00:00:00+00:00',
}

beforeEach(() => {
  vi.clearAllMocks()
  mockGetCreateTicketUrl.mockReturnValue('/tickets')
})

describe('useCreateTicket', () => {
  it('posts the request through the generated API path and returns the ticket', async () => {
    mockPost.mockReturnValue({ json: vi.fn().mockResolvedValue(ticket) })

    const { result } = renderHook(() => useCreateTicket(), { wrapper })

    const created = await result.current.mutateAsync({ userInput: 'Build me a thing' })

    expect(mockGetCreateTicketUrl).toHaveBeenCalled()
    expect(mockPost).toHaveBeenCalledWith('tickets', { json: { userInput: 'Build me a thing' } })
    expect(created).toEqual(ticket)
  })

  it('keeps generated paths without a leading slash unchanged', async () => {
    mockGetCreateTicketUrl.mockReturnValue('tickets')
    mockPost.mockReturnValue({ json: vi.fn().mockResolvedValue(ticket) })

    const { result } = renderHook(() => useCreateTicket(), { wrapper })

    await result.current.mutateAsync({ userInput: 'Build me a thing' })

    expect(mockPost).toHaveBeenCalledWith('tickets', { json: { userInput: 'Build me a thing' } })
  })

  it('invalidates the tickets list query on success', async () => {
    mockPost.mockReturnValue({ json: vi.fn().mockResolvedValue(ticket) })

    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    })
    const invalidate = vi.spyOn(queryClient, 'invalidateQueries')

    function localWrapper({ children }: { children: ReactNode }) {
      return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    }

    const { result } = renderHook(() => useCreateTicket(), { wrapper: localWrapper })

    result.current.mutate({ userInput: 'Build me a thing' })

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true)
    })

    expect(invalidate).toHaveBeenCalledWith({ queryKey: ['tickets'] })
  })
})
