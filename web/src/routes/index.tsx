import { createFileRoute, Link } from '@tanstack/react-router'

export const Route = createFileRoute('/')({
  component: Home,
})

function Home() {
  return (
    <div className="space-y-3">
      <h1 className="text-2xl font-semibold">bouw</h1>
      <p className="text-muted-foreground">
        A vertical-slice React frontend.{' '}
        <Link to="/workflows" className="text-foreground underline">
          View workflows →
        </Link>
      </p>
    </div>
  )
}
