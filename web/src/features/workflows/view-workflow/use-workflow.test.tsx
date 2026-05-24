import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { renderHook, waitFor } from '@testing-library/react'
import type { ReactNode } from 'react'
import type { Mock } from 'vitest'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { api } from '@/lib/api'
import { getGetWorkflowUrl, type WorkflowResponse } from '@/lib/api.generated'

import { useWorkflow, workflowQueryKey } from './use-workflow'

vi.mock('@/lib/api', () => ({
  api: {
    get: vi.fn(),
  },
}))

vi.mock('@/lib/api.generated', () => ({
  getGetWorkflowUrl: vi.fn(),
}))

const mockGet = api.get as unknown as Mock
const mockGetWorkflowUrl = getGetWorkflowUrl as unknown as Mock

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  })

  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
}

const workflow: WorkflowResponse = {
  id: '1',
  key: 'frame',
  name: 'Frame',
  description: 'Frame the work',
  status: 'active',
  steps: [],
}

beforeEach(() => {
  vi.clearAllMocks()
  mockGetWorkflowUrl.mockReturnValue('/workflows/frame')
})

describe('useWorkflow', () => {
  it('loads a workflow using the generated API path', async () => {
    mockGet.mockResolvedValue({
      ok: true,
      status: 200,
      json: vi.fn().mockResolvedValue(workflow),
    })

    const { result } = renderHook(() => useWorkflow('frame'), { wrapper })

    await waitFor(() => { expect(result.current.isSuccess).toBe(true); })

    expect(mockGetWorkflowUrl).toHaveBeenCalledWith('frame')
    expect(mockGet).toHaveBeenCalledWith('workflows/frame', { throwHttpErrors: false })
    expect(workflowQueryKey('frame')).toEqual(['workflow', 'frame'])
    expect(result.current.data).toEqual(workflow)
  })

  it('keeps generated paths without a leading slash unchanged', async () => {
    mockGetWorkflowUrl.mockReturnValue('workflows/frame')
    mockGet.mockResolvedValue({
      ok: true,
      status: 200,
      json: vi.fn().mockResolvedValue(workflow),
    })

    const { result } = renderHook(() => useWorkflow('frame'), { wrapper })

    await waitFor(() => { expect(result.current.isSuccess).toBe(true); })

    expect(mockGet).toHaveBeenCalledWith('workflows/frame', { throwHttpErrors: false })
  })

  it('returns null when the workflow is not found', async () => {
    mockGet.mockResolvedValue({
      ok: false,
      status: 404,
      json: vi.fn(),
    })

    const { result } = renderHook(() => useWorkflow('missing'), { wrapper })

    await waitFor(() => { expect(result.current.isSuccess).toBe(true); })

    expect(result.current.data).toBeNull()
  })

  it('throws for non-404 API failures', async () => {
    mockGet.mockResolvedValue({
      ok: false,
      status: 500,
      json: vi.fn(),
    })

    const { result } = renderHook(() => useWorkflow('frame'), { wrapper })

    await waitFor(() => { expect(result.current.isError).toBe(true); })
    expect(result.current.error).toEqual(new Error("Could not load workflow 'frame'."))
  })
})
