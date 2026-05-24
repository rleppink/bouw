import { useQuery } from '@tanstack/react-query'

import { api } from '@/lib/api'
import { getGetWorkflowUrl, type WorkflowResponse } from '@/lib/api.generated'

export const workflowQueryKey = (key: string) => ['workflow', key] as const

function toKyPath(path: string): string {
  return path.startsWith('/') ? path.slice(1) : path
}

async function fetchWorkflow(key: string): Promise<WorkflowResponse | null> {
  const response = await api.get(toKyPath(getGetWorkflowUrl(key)), {
    throwHttpErrors: false,
  })

  if (response.status === 404) {
    return null
  }

  if (!response.ok) {
    throw new Error(`Could not load workflow '${key}'.`)
  }

  return response.json<WorkflowResponse>()
}

export function useWorkflow(key: string) {
  return useQuery({
    queryKey: workflowQueryKey(key),
    queryFn: () => fetchWorkflow(key),
  })
}
