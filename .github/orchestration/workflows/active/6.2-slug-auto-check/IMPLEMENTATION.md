# IMPLEMENTATION

## Planned Changes

- Add debounced slug availability checking.
- Keep manual Check button behavior.
- Update desktop slug controls to one row.
- Verify frontend checks.

## Files Touched

- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- `README.md`

## Verification

- `npm run check --prefix frontend`

## Notes

- Backend changes are not expected.
- Debounce delay is `700ms`.
