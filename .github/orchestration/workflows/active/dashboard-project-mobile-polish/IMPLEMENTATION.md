# IMPLEMENTATION

## Planned Changes

- Refine dashboard row and action layout for small screens.
- Refine project page control groups, upload area, QR section, and action sections for mobile.

## Files Touched

- `frontend/src/routes/dashboard/+page.svelte`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`

## Verification

- `npm run check` in `frontend/`

## Notes

- Dashboard rows now stack more naturally on narrow screens and keep the eye action aligned without crowding.
- Project settings action groups, slug controls, upload actions, and danger zone now favor stronger vertical flow on mobile.
- Frontend-only responsiveness pass.
