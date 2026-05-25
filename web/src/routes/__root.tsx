import { createRootRoute, Link, Outlet } from '@tanstack/react-router'

export const Route = createRootRoute({
  component: RootLayout,
})

function RootLayout() {
  return (
    <div className="mx-auto max-w-7xl p-6">
      <header className="mb-8 flex items-center gap-4 border-b pb-4">
        <Link to="/" className="font-semibold">
          bouw
        </Link>
        <nav className="text-muted-foreground flex gap-3 text-sm">
          <Link to="/workflows" className="[&.active]:text-foreground [&.active]:underline">
            Workflows
          </Link>
          <Link to="/board" className="[&.active]:text-foreground [&.active]:underline">
            Board
          </Link>
        </nav>
      </header>
      <Outlet />
    </div>
  )
}
