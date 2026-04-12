# IMPLEMENTATION

## Planned Changes

- Add a QR generation dependency.
- Build a QR builder UI section in the project editor.
- Wire live preview and download behavior.

## Files Touched

- `frontend/package.json`
- `frontend/src/lib/components/ProjectQrBuilder.svelte`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`

## Verification

- `npm run check` in `frontend/`

## Notes

- Added a frontend-only QR builder with style presets, color controls, live preview, and PNG/SVG download.
- The QR target is derived from the current slug value in the editor.
- Frontend-only slice.
