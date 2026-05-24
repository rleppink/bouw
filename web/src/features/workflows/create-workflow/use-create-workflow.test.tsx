import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { renderHook, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import type { Mock } from 'vitest'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { api } from '@/lib/api'

import { useCreateWorkflow } from './use-create-workflow'

vi.mock('@/lib/api', () => ({
  api: {
    post: vi.fn(),
  },
}))

const mockPost = api.post as unknown as Mock

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      mutations: { retry: false },
      queries: { retry: false },
    },
  })
  const invalidateQueries = vi.spyOn(queryClient, 'invalidateQueries')

  function wrapper({ children }: { children: ReactNode }) {
    return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  }

  return { invalidateQueries, wrapper }
}

beforeEach(() => {
  vi.clearAllMocks()
})

describe('useCreateWorkflow', () => {
  it('posts the workflow and invalidates the workflow list on success', async () => {
    mockPost.mockReturnValue({
      json: vi.fn().mockResolvedValue({ id: '1', name: 'Foundation pour' }),
    })
    const { invalidateQueries, wrapper } = createWrapper()

    const { result } = renderHook(() => useCreateWorkflow(), { wrapper })

    await result.current.mutateAsync({ name: 'Foundation pour', status: 'draft' })

    expect(mockPost).toHaveBeenCalledWith('workflows', {
      json: { name: 'Foundation pour', status: 'draft' },
    })
    await waitFor(() => {
      expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: ['workflows'] })
    })
  })

  it('surfaces invalid create responses as mutation errors', async () => {
    mockPost.mockReturnValue({
      json: vi.fn().mockResolvedValue({ id: 1 }),
    })
    const { wrapper } = createWrapper()

    const { result } = renderHook(() => useCreateWorkflow(), { wrapper })

    await expect(result.current.mutateAsync({ name: 'Foundation pour', status: 'draft' })).rejects
      .toThrow()
  })
})
