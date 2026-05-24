import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

import { useWorkflow } from './use-workflow'

type ViewWorkflowProps = {
  workflowId: string
}

export function ViewWorkflow({ workflowId }: ViewWorkflowProps) {
  const query = useWorkflow(workflowId)

  if (query.isPending) {
    return <p role="status">Loading workflow...</p>
  }

  if (query.isError) {
    return <p role="alert">Could not load workflow.</p>
  }

  if (!query.data) {
    return <p role="alert">Workflow not found.</p>
  }

  const workflow = query.data

  return (
    <section className="space-y-6">
      <header className="space-y-2">
        <div className="text-muted-foreground text-sm">{workflow.status}</div>
        <h1 className="text-2xl font-semibold">{workflow.name}</h1>
        <p className="text-muted-foreground">{workflow.description}</p>
      </header>

      <div className="space-y-3">
        <h2 className="text-xl font-semibold">Steps</h2>
        {workflow.steps.length === 0 ? (
          <p className="text-muted-foreground text-sm">This workflow has no steps.</p>
        ) : (
          <ol className="space-y-3">
            {workflow.steps.map((step) => (
              <li key={step.id}>
                <Card>
                  <CardHeader>
                    <CardTitle className="text-base">
                      {step.position}. {step.name}
                    </CardTitle>
                    <div className="text-muted-foreground text-sm">{step.key}</div>
                  </CardHeader>
                  <CardContent>
                    {step.actions.length === 0 ? (
                      <p className="text-muted-foreground text-sm">No actions.</p>
                    ) : (
                      <ul className="space-y-2">
                        {step.actions.map((action) => (
                          <li key={action.id} className="rounded-md border p-3">
                            <div className="flex flex-wrap items-center gap-2 text-sm">
                              <span className="font-medium">
                                {action.position}. {action.key}
                              </span>
                              <span className="text-muted-foreground">{action.type}</span>
                            </div>
                            <pre className="bg-muted mt-2 overflow-auto rounded px-3 py-2 text-xs">
                              {action.configJson}
                            </pre>
                          </li>
                        ))}
                      </ul>
                    )}
                  </CardContent>
                </Card>
              </li>
            ))}
          </ol>
        )}
      </div>
    </section>
  )
}
