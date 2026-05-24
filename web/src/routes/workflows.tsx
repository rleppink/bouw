import { createFileRoute } from '@tanstack/react-router'

import { ListWorkflows } from '@/features/workflows/list-workflows/list-workflows'

export const Route = createFileRoute('/workflows')({
  component: WorkflowsPage,
})

// The route is the composition layer (analogous to the backend's Endpoint
// wiring): it may depend on multiple slices. The slices themselves never depend
// on each other.
function WorkflowsPage() {
  return (
    <section className="space-y-3">
      <h1 className="text-xl font-semibold">Workflows</h1>
      <ListWorkflows />
    </section>
  )
}
