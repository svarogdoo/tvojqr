# SPEC

## Scope

Implement the first authenticated image upload path for the default project language, including persistence and dashboard display.

## Affected Areas

- backend domain/application/infrastructure for assets
- backend API project endpoints
- frontend project settings page

## Technical Approach

- Add a minimal asset model linked to project ownership.
- Store asset metadata in PostgreSQL.
- Persist uploaded files locally on disk for now to keep the slice small and testable.
- Add an authenticated multipart upload endpoint scoped to the current project.
- Show uploaded assets on the project page after refresh/upload.

## Assumptions

- Image-only uploads are enough for this slice.
- Local file storage is acceptable as a temporary development step before object storage.
- Language variants remain a follow-up task.

## Risks

- Local file storage is not the long-term hosting strategy.
- Image conversion/compression is not yet included if it materially complicates this first upload slice.
