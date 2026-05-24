import { createFileRoute } from '@tanstack/react-router'

import { ViewWorkflow } from '@/features/workflows/view-workflow/view-workflow'

export const Route = createFileRoute('/workflows_/$key')({
  component: WorkflowPage,
})

function WorkflowPage() {
  const { key } = Route.useParams()
  return <ViewWorkflow workflowKey={key} />
}
