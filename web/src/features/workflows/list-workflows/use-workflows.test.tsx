import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { renderHook, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import type { Mock } from 'vitest'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { api } from '@/lib/api'

import { useWorkflows, workflowsQueryKey } from './use-workflows'

vi.mock('@/lib/api', () => ({
  api: {
    get: vi.fn(),
  },
}))

const mockGet = api.get as unknown as Mock

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })

  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useWorkflows', () => {
  it('loads workflows through the API client and parses the response', async () => {
    const data = [
      {
        id: '1',
        name: 'Foundation pour',
        status: 'active',
        createdAt: '2026-01-02T00:00:00.000Z',
      },
    ]
    mockGet.mockReturnValue({ json: vi.fn().mockResolvedValue(data) })

    const { result } = renderHook(() => useWorkflows(), { wrapper })

    await waitFor(() => { expect(result.current.isSuccess).toBe(true); })

    expect(mockGet).toHaveBeenCalledWith('workflows')
    expect(workflowsQueryKey).toEqual(['workflows'])
    expect(result.current.data).toEqual(data)
  })

  it('surfaces invalid API data as a query error', async () => {
    mockGet.mockReturnValue({ json: vi.fn().mockResolvedValue([{ id: '1' }]) })

    const { result } = renderHook(() => useWorkflows(), { wrapper })

    await waitFor(() => { expect(result.current.isError).toBe(true); })
  })
})
