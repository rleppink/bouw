import { useQuery } from '@tanstack/react-query'

import { api } from '@/lib/api'
import { getGetWorkflowUrl, type WorkflowResponse } from '@/lib/api.generated'

export const workflowQueryKey = (id: string) => ['workflow', id] as const

function toKyPath(path: string): string {
  return path.startsWith('/') ? path.slice(1) : path
}

async function fetchWorkflow(id: string): Promise<WorkflowResponse | null> {
  const response = await api.get(toKyPath(getGetWorkflowUrl(id)), {
    throwHttpErrors: false,
  })

  if (response.status === 404) {
    return null
  }

  if (!response.ok) {
    throw new Error(`Could not load workflow '${id}'.`)
  }

  return response.json<WorkflowResponse>()
}

export function useWorkflow(id: string) {
  return useQuery({
    queryKey: workflowQueryKey(id),
    queryFn: () => fetchWorkflow(id),
  })
}
