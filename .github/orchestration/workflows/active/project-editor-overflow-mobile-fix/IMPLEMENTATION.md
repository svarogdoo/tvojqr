# IMPLEMENTATION

## Planned Changes

- Tighten project editor layout for small screens.
- Reduce control overflow in slug, uploads, QR, and action areas.

## Files Touched

- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- `frontend/src/lib/components/ProjectQrBuilder.svelte`

## Verification

- `npm run check` in `frontend/`

## Notes

- Reduced horizontal pressure in the slug control by stacking the prefix/input and action buttons more aggressively on small screens.
- Relaxed image grid and QR builder layout so they fit smaller widths without overflow.
- Frontend-only responsiveness bugfix.
