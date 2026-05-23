import { useMutation, useQueryClient } from '@tanstack/react-query'

import { api } from '@/lib/api'

import { createdWorkflowSchema, type CreateWorkflowInput } from './create-workflow.contracts'

export function useCreateWorkflow() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (input: CreateWorkflowInput) => {
      const data = await api.post('workflows', { json: input }).json()
      return createdWorkflowSchema.parse(data)
    },
    onSuccess: async () => {
      // Refresh the workflows list. The key is duplicated on purpose: slices
      // never import one another, so this slice does not reach into
      // list-workflows for its query key.
      await queryClient.invalidateQueries({ queryKey: ['workflows'] })
    },
  })
}
