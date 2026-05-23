# Workflow Data Shape

## Summary

Model the reusable recipe as a `Workflow`, and model one active user collaboration
as a `Session`. A session runs through ordered workflow steps, executes step
actions sequentially, and stores collaborative "files" as database-backed
documents owned by the session.

## Core Model

- `Workflow`
  - Code-defined recipe, not user-authored JSON for the MVP.
  - Fields: `id`, `key`, `name`, `description`, `status`, ordered `steps`.
  - Example keys: `frame`, `plan`, later `implement`.

- `Step`
  - A recipe step inside a workflow.
  - Fields: `id`, `workflowId`, `key`, `name`, `position`, ordered `actions`.
  - Example: `interview`, `synthesize`, `draft_plan`.

- `Action`
  - A recipe action inside a step.
  - Fields: `id`, `stepId`, `key`, `type`, `position`, typed config.
  - Initial action types:
    - `ask_user_input`
    - `call_llm`
    - `edit_document`

- `Session`
  - One execution/collaboration started from a workflow.
  - Fields: `id`, `workflowId`, `title`, `status`, `currentStepKey`, `createdAt`,
    `updatedAt`.
  - Statuses: `draft`, `active`, `waiting_for_user`, `complete`, `failed`,
    `archived`.

- `SessionStep`
  - Runtime state for a step inside one session.
  - Fields: `id`, `sessionId`, `stepKey`, `status`, `startedAt`, `completedAt`.

- `ActionRun`
  - Runtime record for one action execution.
  - Fields: `id`, `sessionId`, `stepKey`, `actionKey`, `status`, `inputJson`,
    `outputJson`, `error`, `startedAt`, `completedAt`.
  - Actions within a step run sequentially.

- `Document`
  - Database-backed collaborative text "file".
  - Belongs to the session, not a specific step.
  - Fields: `id`, `sessionId`, `kind`, `title`, `contentMarkdown`, `version`,
    `createdBy`, `createdAt`, `updatedAt`.
  - Example `kind`s: `framing`, `plan`, `implementation_notes`.

## Behavior

- Starting a workflow creates a `Session` and initializes `SessionStep` rows from
  the code-defined `Workflow`.
- The orchestrator advances one action at a time.
- `ask_user_input` pauses the session as `waiting_for_user`.
- `call_llm` assembles context from session messages/documents, sends one
  stateless prompt, and stores the response in `ActionRun.outputJson`.
- `edit_document` creates or updates a session `Document`; no filesystem writes
  in the MVP.
- Later steps reference session documents by `kind`, for example the plan step
  reads the `framing` document.

## Test Scenarios

- Starting a workflow creates a session with ordered session steps.
- Actions execute sequentially inside a step.
- User-input actions pause the session until input is submitted.
- LLM actions persist their input/output without controlling flow.
- Document actions create and update session-owned markdown documents.
- Later actions can read prior session documents by `kind`.

## Assumptions

- `Workflow + Session` is the preferred naming style.
- `Step + SessionStep` is preferred over `StepDefinition + StepRun`.
- Workflows are code-defined for the MVP.
- Documents are database records, not files on disk.
- No branching, DAG execution, parallel actions, or user-authored workflow
  definitions in the MVP.
