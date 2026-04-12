# INTENT

## User Request

After deleting a project, the public slug still behaves as if the project were disabled. Fix that so deleted projects no longer resolve.

## Requested Outcome

Deleted projects should return the missing/not-found public state, not the disabled state.

## Constraints

- Focus on the delete/public lookup bug only.
- Verification is required.

## Open Questions

- None for this bugfix.
