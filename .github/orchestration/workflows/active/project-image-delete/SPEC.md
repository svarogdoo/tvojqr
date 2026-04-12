# SPEC

## Scope

Add single-image deletion for project assets across backend and frontend.

## Affected Areas

- backend asset repository/service/storage
- project asset upload/list section in the frontend

## Technical Approach

- Add a delete endpoint scoped to the current user and project.
- Delete the file from storage and remove the asset row.
- Add a small corner bin button overlay to each image card.

## Assumptions

- Per-image delete is enough; bulk delete is unnecessary now.

## Risks

- File-delete failures should not leave metadata out of sync.
