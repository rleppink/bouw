import { useQuery } from '@tanstack/react-query'

import { api } from '@/lib/api'
import { getGetWorkflowsUrl } from '@/lib/api.generated'

import { workflowListSchema, type WorkflowSummary } from './workflow.contracts'

export const workflowsQueryKey = ['workflows'] as const

function toKyPath(path: string): string {
  return path.startsWith('/') ? path.slice(1) : path
}

async function fetchWorkflows(): Promise<WorkflowSummary[]> {
  const data = await api.get(toKyPath(getGetWorkflowsUrl())).json()
  // Parse at the boundary: the API response is external data.
  return workflowListSchema.parse(data)
}

export function useWorkflows() {
  return useQuery({
    queryKey: workflowsQueryKey,
    queryFn: fetchWorkflows,
  })
}
