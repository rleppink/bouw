# Workflow Id Refactor

## Status

Done on 2026-05-24.

Completed changes:

- `Workflow.Id` is the canonical workflow identity for the get-workflow API,
  generated client, frontend route, query key, and workflow navigation.
- The backend route is `/workflows/{id:guid}`.
- Top-level `Workflow.Key` is removed from the entity, EF configuration,
  response DTO, OpenAPI schema, generated frontend client, and migration model.
- `WorkflowStep.Key` and `WorkflowAction.Key` remain in place for runtime step
  and action identity.

Verification:

- `dotnet test Bouw.sln`
- `npm run typecheck`
- `npm test`
- `npm run lint`

## Summary

Workflows will be user-created, edited, and maintained. Because of that,
`Workflow.Id` should be the canonical workflow identity everywhere, and the
top-level `Workflow.Key` should be removed rather than treated as a slug or
alternate identifier.

This supersedes the earlier assumption that workflows are primarily
code-defined catalog items with stable product keys such as `ship-feature`.

## Intention

- Workflows are ordinary persisted user data.
- Users can create workflows.
- Users can edit workflows.
- Users can maintain workflows over time.
- The workflow row id is the stable identity for lookups, links, API calls,
  joins, and logs.
- The application should not require users or code to manage a separate
  workflow key.
- Top-level `Workflow.Key` should be removed.

## Route Shape

Use the workflow id in workflow routes:

```text
GET /workflows/{id}
```

The existing key-based shape should be retired:

```text
GET /workflows/{key}
```

The current seeded URL:

```text
GET /workflows/ship-feature
```

is only a temporary artifact of the earlier code-defined workflow setup.

## Why Not Keep `Workflow.Key`

For user-created workflows, a separate key behaves like a slug. Slugs introduce
product and data-management questions that are not needed right now:

- how to generate unique slugs
- what happens when a workflow is renamed
- whether URLs should change after rename
- how to handle collisions
- whether users can edit slugs
- which words are reserved

The id already solves identity without those extra rules.

## What Should Stay

`WorkflowStep.Key` and `WorkflowAction.Key` should stay for now.

Those keys are not workflow identities. They are stable names inside a workflow
definition and are used by runtime state such as:

- `WorkflowSession.CurrentStepKey`
- `SessionStep.StepKey`
- `ActionRun.StepKey`
- `ActionRun.ActionKey`

That means the intended refactor is specifically:

```text
Workflow.Id          keep and use as canonical identity
Workflow.Key         remove
WorkflowStep.Key     keep
WorkflowAction.Key   keep
```

## Implementation Notes

- Change `GetWorkflow` from lookup by `Workflow.Key` to lookup by `Workflow.Id`.
- Change the backend route constraint to a GUID id, for example
  `/workflows/{id:guid}`.
- Update generated OpenAPI/client naming so the parameter is `id`, not `key`.
- Update frontend route props, query keys, and navigation to use workflow ids.
- Remove `Workflow.Key` from the entity, configuration, response DTOs, seed
  logic, and migrations as part of the schema cleanup.
- Revisit the development seed workflow after the model changes. It can still
  exist as sample data, but it should not depend on a stable top-level key.
