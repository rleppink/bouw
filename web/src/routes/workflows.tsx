import { createFileRoute } from '@tanstack/react-router'

import { CreateWorkflowForm } from '@/features/workflows/create-workflow/create-workflow-form'
import { ListWorkflows } from '@/features/workflows/list-workflows/list-workflows'

export const Route = createFileRoute('/workflows')({
  component: WorkflowsPage,
})

// The route is the composition layer (analogous to the backend's Endpoint
// wiring): it may depend on multiple slices. The slices themselves never depend
// on each other.
function WorkflowsPage() {
  return (
    <div className="space-y-10">
      <section className="space-y-3">
        <h2 className="text-xl font-semibold">New workflow</h2>
        <CreateWorkflowForm />
      </section>
      <section className="space-y-3">
        <h2 className="text-xl font-semibold">Workflows</h2>
        <ListWorkflows />
      </section>
    </div>
  )
}
