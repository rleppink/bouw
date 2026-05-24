import { Link } from '@tanstack/react-router'

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'

import { formatWorkflowDate } from './format-workflow-date'
import { useWorkflows } from './use-workflows'
import { matchesSearch, useWorkflowFilter } from './workflow-filter.store'

export function ListWorkflows() {
  const query = useWorkflows()
  const search = useWorkflowFilter((state) => state.search)
  const setSearch = useWorkflowFilter((state) => state.setSearch)

  if (query.isPending) {
    return <p role="status">Loading workflows…</p>
  }

  if (query.isError) {
    return <p role="alert">Could not load workflows.</p>
  }

  const visible = query.data.filter((workflow) => matchesSearch(workflow.name, search))

  return (
    <section className="space-y-4">
      <Input
        type="search"
        aria-label="Filter workflows"
        placeholder="Filter workflows…"
        value={search}
        onChange={(event) => {
          setSearch(event.target.value)
        }}
      />

      {visible.length === 0 ? (
        <p className="text-muted-foreground text-sm">No workflows match your filter.</p>
      ) : (
        <ul className="space-y-2">
          {visible.map((workflow) => (
            <li key={workflow.id}>
              <Link to="/workflows/$id" params={{ id: workflow.id }} className="block">
                <Card className="hover:bg-muted/50 transition-colors">
                  <CardHeader>
                    <CardTitle>{workflow.name}</CardTitle>
                  </CardHeader>
                  <CardContent className="text-muted-foreground text-sm">
                    {workflow.status} · created {formatWorkflowDate(workflow.createdAt)}
                  </CardContent>
                </Card>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
