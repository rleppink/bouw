# bouw architecture

`Bouw.API` is organised as **vertical slices**: one folder per operation
(`CreateWorkflow`, `EditWorkflow`, …), each owning its HTTP entrypoint, its
business logic, and its contracts. There is no horizontal Controller → Service →
Repository stack. The unit of change is the slice.

It is a **single bounded context with a shared data model**. Entities and the
`DbContext` live in one shared `Persistence/` layer; every slice queries and
writes it directly. The one hard architectural rule is: **a slice never depends
on another slice.** Almost everything else follows from those two sentences.

## Structure

> Illustrative — names like `Workflows` / `CreateWorkflow` show the *shape*, not
> what to build. Real slices are added as the domain demands.

```
api/                                  (root namespace Bouw.API)
├── Program.cs                        // builds the app; calls app.MapFeatures()
├── Features/                         // all slices live here
│   └── Workflows/                    // topical grouping only — NOT a boundary
│       ├── CreateWorkflow/           // a SLICE = one operation
│       │   ├── Endpoint.cs           // route + filters (HTTP only)
│       │   ├── Handler.cs            // the business logic for this op
│       │   └── Contracts.cs          // request/response records
│       └── EditWorkflow/
│           └── …
├── Persistence/                      // the shared data model — used by every slice
│   ├── Entities/                     // Workflow.cs, User.cs, …
│   ├── Configurations/               // EF type configurations
│   ├── BouwDbContext.cs
│   └── Migrations/
└── Infrastructure/                   // shared technical integrations
    └── (LLM client + prompt loader — added when first needed)
```

Namespaces are file-scoped and mirror the folders:
`Bouw.API.Features.Workflows.CreateWorkflow`.

The middle grouping folder (`Workflows/`) is **organizational only** — a place to
find related slices. It owns nothing and enforces nothing; slices inside it are as
independent from one another as slices anywhere else.

## Inside a slice

Three concerns are the usual shape:

- **`Endpoint.cs`** — maps one route (under a `MapGroup` prefix) and attaches
  filters. HTTP plumbing only; no logic.
- **`Handler.cs`** — a `sealed` class, DI-injected with `BouwDbContext` and
  whatever else it needs. **This is where business logic lives.** Thin for CRUD is
  fine; the value is that there is *exactly one* obvious place per operation.
- **`Contracts.cs`** — request/response `record`s. Distinct from entities so the
  wire shape and the stored shape can diverge.

There is **no separate Service or Logic layer** inside a slice — that would
re-introduce the horizontal layering VSA exists to remove.

A slice also holds whatever *only it* uses — a slice-private helper, a small
algorithm or data structure (`internal` / `file`-scoped, so the no-slice→slice
rule fences it in automatically). When to lift such a helper out depends on *what
kind* of code it is, not merely on a second slice wanting something like it:

- **Stable, domain-agnostic utilities** (a `BinaryHeap`) — promote to `Common/`
  once a second slice genuinely needs the same thing. They carry no domain
  meaning, so sharing them cannot couple features.
- **Domain or behavioural code** that merely looks similar across slices — do
  *not* promote. Duplicate it; the two are different concepts that will diverge,
  and hoisting them into a shared home is the wrong-abstraction trap.

## Dispatch

Direct. The minimal-API endpoint takes the handler as a DI parameter and calls
it — no mediator library.

```csharp
app.MapPost("/workflows",
    async (CreateWorkflowRequest req, CreateWorkflowHandler handler, CancellationToken ct)
        => await handler.Handle(req, ct));
```

## Persistence (the shared data model)

All entities, their EF configurations, the `DbContext`, and migrations live in one
shared `Persistence/` layer. **It is deliberately shared** — any slice reads and
writes it directly. There is no per-feature data ownership and no private
entities: inside a single bounded context the data model is common ground.

- **Reads** project freely into the slice's own response shape; use
  `AsNoTracking()` (or set it as the context default) for read-only queries.
- **Writes** go through the relevant entity and `SaveChangesAsync()`. Domain rules
  that must always hold can live as methods on the entity
  (`Workflow.AddStage()` rejecting duplicates); use-case orchestration stays in the
  `Handler`.
- **Cross-topic queries are fine** — a slice grouped under `Workflows/` may read
  `User` data straight from the context. There is no wall to cross.

## Sharing & boundaries

One hard rule, enforced: **a slice never references another slice.** That is what
keeps a slice deletable and comprehensible in isolation.

Beyond that:

- **Data is shared; behaviour is not.** The shared layer is the *data model*, not
  a shared Service layer. Don't hoist business logic into shared helpers to avoid
  repeating it.
- **Prefer duplication over the wrong abstraction.** Two slices with similar
  mapping or validation duplicate it and are free to diverge; extract on the
  rule-of-three, or when correctness demands one definition.
- **Extract only stable, cross-cutting infrastructure** to `Infrastructure/` — the
  LLM client, auth, time. Created lazily, never up front.

## Enforcement

ArchUnitNET rules in the test project:

- no slice references another slice (the core invariant);
- dependencies point one way: `Features` → `Persistence` / `Infrastructure`, never
  back;
- every `IEndpoint` is reachable from `MapFeatures`.

New slices register themselves: `Endpoint.cs` implements `IEndpoint`
(`static abstract void Map(...)`), and a single `MapFeatures()` extension
reflection-scans the assembly and wires them all. **Adding a slice never edits
`Program.cs`.**

## Deliberately not used

- **MediatR / a mediator** — direct DI removes the need (and the licence).
- **Per-slice Service / Logic layers** — collapsed into `Handler`.
- **Per-feature data ownership / bounded-context isolation** — we considered the
  modular-monolith path (private entities, published read contracts, ports,
  integration events — i.e. the DDD context-mapping patterns) and chose against
  it: `bouw` is one bounded context with a shared model. Revisit only if a part
  must become independently deployable.
- **FluentValidation / a validation pipeline** — start with in-handler guards or
  an endpoint filter; add a library only when duplication earns it.
- **`Result<T>` / error monad** — minimal APIs return typed `Results<…>` first.
