# IMPLEMENTATION

## Planned Changes

- Add unsaved-change detection.
- Add route/browser leave warnings.
- Emphasize save CTA when dirty.
- Show dashboard snackbar after successful save navigation.

## Files Touched

- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- `README.md`

## Verification

- `npm run check --prefix frontend`

## Notes

- Backend changes are not expected.
- Save success snackbar now shows after dashboard navigation completes.
