# IMPLEMENTATION

## Planned Changes

- Simplify the project editor layout.
- Add a new-tab public-view action to dashboard rows.

## Files Touched

- `frontend/src/routes/dashboard/+page.svelte`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`

## Verification

- `npm run check` in `frontend/`

## Notes

- The project editor now uses a single focused main column.
- Dashboard rows now include a dedicated public-view eye action opening the slug page in a new tab.
- This task is UI polish only.
