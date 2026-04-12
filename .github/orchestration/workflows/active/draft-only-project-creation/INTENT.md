# INTENT

## User Request

Switch the new-project flow so nothing is saved to the database until the user explicitly saves. Images should stay in the frontend until then, with local previews.

## Requested Outcome

`New project` opens a frontend-only draft editor. Project creation and image upload happen only after explicit save.

## Constraints

- Losing unsaved work on refresh/close is acceptable.
- Existing saved-project editing should keep working.
- Verification is required.

## Open Questions

- None for this slice.
