# Docker Dev Setup

## Summary

Use Docker Compose for localized whole-app testing of `api/` and `web/`.
Postgres should run as a normal Compose sibling service, not Docker-in-Docker.
Each git worktree gets its own Compose project and database. Docker publishes
the web app on an ephemeral localhost port so multiple worktrees can run
independently.

The default workflow should optimize for testing a worktree's current source
changes quickly:

- API runs from a .NET SDK container against bind-mounted `api/` source.
- Web runs from a Node container against bind-mounted `web/` source.
- Postgres is internal to the Compose project.
- The browser opens the localhost URL assigned by Docker, such as
  `http://localhost:49153`.

## Core Setup

- Add a root `compose.yaml` with three services:
  - `postgres`: official Postgres image, scoped to the Compose project, with a
    healthcheck.
  - `api`: pinned .NET 10 SDK image, such as
    `mcr.microsoft.com/dotnet/sdk:10.0`, runs the API on `http://+:8080`.
  - `web`: pinned Node image compatible with Vite's engine requirements, such
    as `node:22-bookworm-slim`, runs Vite on `0.0.0.0:5173`.

- Use fixed local Postgres database, user, and password values in
  `compose.yaml`. This database is disposable and scoped to the Compose project.

- Add `scripts/docker-dev` to derive the Compose project name from the current
  git branch plus a short hash of the worktree path, and wrap common Docker
  Compose commands.
  - Example branch `main` in worktree `/Users/me/src/bouw` becomes a project
    like `bouw-main-a1b2c3`.
  - Example branch `feature/docker-dev` in worktree
    `/Users/me/src/bouw-docker-dev` becomes a project like
    `bouw-feature-docker-dev-9f8e7d`.
  - If the worktree is detached, fall back to the worktree directory name.

- Publish the web service on an ephemeral localhost port:
  - `127.0.0.1::5173`

The tradeoff is that the browser URL includes a port, and you need to ask Docker
which port was assigned.

### Port Setup

Each worktree gets a deterministic Compose project name from its current git
branch and worktree path:

```sh
branch_or_dir="$(git branch --show-current)"
path_hash="$(pwd -P | shasum | cut -c1-6)"
project="bouw-${branch_or_dir}-${path_hash}"
```

The script must sanitize the branch name for Docker Compose:

- Lowercase it.
- Replace non-alphanumeric runs with `-`.
- Trim leading and trailing `-`.
- Prefix with `bouw-`.
- Append a short stable hash of the absolute worktree path.

After starting the stack, ask Docker which host port it assigned, using the same
project name:

```sh
docker compose -p bouw-main port web 5173
```

Example output:

```text
127.0.0.1:49153
```

The app URL for that worktree is then:

```text
http://localhost:49153
```

Docker chooses the host port automatically. The container still listens on
`5173`; only the host-side port changes.

## App Wiring

- Postgres should not publish a host port by default.
  - The API connects to `postgres:5432` on the Compose network.
  - This prevents conflicts across worktrees and other local Postgres instances.
  - Use `pg_isready` for the Postgres healthcheck.

- Configure the API container with:
  - `ASPNETCORE_ENVIRONMENT=Development`
  - `ASPNETCORE_URLS=http://+:8080`
  - `ConnectionStrings__Bouw=Host=postgres;Port=5432;Database=...`

- Keep the API's existing Development startup behavior for migrations and seed
  data.
  - `DevelopmentDatabaseSeeder.SeedAsync(...)` already applies EF Core
    migrations before seeding.
  - Do not add a Docker-specific migration environment variable for the MVP.
  - Ensure the API waits for Postgres readiness before startup by using the
    Postgres healthcheck and Compose dependency conditions.

- Configure Vite to proxy API calls:
  - Browser requests go to `http://localhost:<assigned-port>/api/...`.
  - Vite proxies `/api` to `http://api:8080` inside Compose.
  - The proxy strips the `/api` prefix before forwarding.

- Set `VITE_API_URL=/api/` for the web container.
  - This avoids CORS because browser traffic goes through the web origin.
  - The API does not need to be exposed on a host port.

- Start the web container with an explicit install step:
  - Run `npm ci` on container startup.
  - Then run Vite with host binding, for example
    `npm run dev -- --host 0.0.0.0`.
  - This keeps the container aligned with the bind-mounted worktree source and
    lockfile.

- Use named volumes for dependency/cache directories that should not be hidden
  or recreated unnecessarily by bind mounts:
  - Web: `/app/node_modules`.
  - API: NuGet package cache.

## Commands

Start a worktree:

```sh
scripts/docker-dev up
```

`up` initializes and starts the whole stack for the current worktree. It should
create the Compose project if needed, start Postgres, wait for readiness through
Compose health/dependency wiring, start the API, run the web install step, and
start Vite. After the stack is up, it should query Docker for the assigned web
host port and print the full app URL, for example:

```text
App URL: http://localhost:49153
```

Print the app URL:

```sh
scripts/docker-dev url
```

Stop a worktree:

```sh
scripts/docker-dev down
```

`down` stops the current worktree's stack and removes project-owned containers,
networks, images built by Compose, and volumes. This makes the next `up`
initialize from a clean disposable database.

The wrapper should use the derived project name with Compose cleanup flags,
roughly:

```sh
docker compose -p "$project" down --volumes --rmi local --remove-orphans
```

The script should print the derived Compose project name before running commands
so it is clear which worktree stack is being controlled.

## Test Scenarios

- Start the stack from one worktree, discover the assigned web port with
  the URL printed by `scripts/docker-dev up`, and verify the corresponding
  localhost URL loads the web app.
- Verify web requests to `/api` reach the API container.
- Verify the API connects to the Compose Postgres service.
- Verify migrations apply against a clean database.
- Verify `scripts/docker-dev down` removes the worktree's disposable database
  volume and Compose-owned containers/images.
- Start a second worktree with a different Compose project name and verify
  Docker assigns it another web host port.
- Verify both worktrees can run at the same time.
- Verify no Postgres host port conflict occurs.

## Assumptions

- No Playwright setup is included yet.
- Database state is disposable for this local testing workflow.
- Branch names plus worktree path hashes can be sanitized into stable Docker
  Compose project names.
- Postgres runs as a Compose service, not Docker-in-Docker.

## Original Requests And Preferences

Things requested:

- Create a Docker setup plan for both `api/` and `web/`.
- Support localized testing of the whole app.
- Support use from git worktrees.
- Keep each worktree's running app separate from other worktree instances.

Things wanted:

- Be able to create a new worktree, spin up that worktree's changes, and verify
  them.
- Prefer a hot-reload-style workflow if that fits the goal.
- Use reset-by-default database behavior for local testing.
- Leave room for later automated browser testing against a worktree.

Things not wanted:

- Do not add Playwright setup to this plan.
- Do not rely on Docker-in-Docker for Postgres.
