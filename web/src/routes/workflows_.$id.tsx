import { createFileRoute } from '@tanstack/react-router'

import { ViewWorkflow } from '@/features/workflows/view-workflow/view-workflow'

export const Route = createFileRoute('/workflows_/$id')({
  component: WorkflowPage,
})

function WorkflowPage() {
  const { id } = Route.useParams()
  return <ViewWorkflow workflowId={id} />
}
