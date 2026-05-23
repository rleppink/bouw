import { useQuery } from '@tanstack/react-query'

import { api } from '@/lib/api'

import { workflowListSchema, type WorkflowSummary } from './workflow.contracts'

export const workflowsQueryKey = ['workflows'] as const

async function fetchWorkflows(): Promise<WorkflowSummary[]> {
  const data = await api.get('workflows').json()
  // Parse at the boundary: the API response is external data.
  return workflowListSchema.parse(data)
}

export function useWorkflows() {
  return useQuery({
    queryKey: workflowsQueryKey,
    queryFn: fetchWorkflows,
  })
}
