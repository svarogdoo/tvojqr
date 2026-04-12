# SPEC

## Scope

Refactor project asset handling so uploads and deletes are staged in the frontend until explicit save.

## Affected Areas

- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- existing project save/upload/delete sequencing

## Technical Approach

- Maintain a staged asset view in local state for both saved and draft projects.
- Track newly added local files separately from saved assets marked for deletion.
- On save:
  - create/update project settings first
  - apply staged deletions for saved assets
  - upload staged new files
- Keep existing backend endpoints where possible.

## Assumptions

- Immediate backend delete/upload is no longer desired in the editor flow.

## Risks

- State reconciliation must avoid losing the distinction between persisted assets and newly staged local files.
