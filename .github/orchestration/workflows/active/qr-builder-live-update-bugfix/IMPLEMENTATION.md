# IMPLEMENTATION

## Planned Changes

- Fix QR builder live re-rendering.
- Tighten the QR builder layout.

## Files Touched

- `frontend/src/lib/components/ProjectQrBuilder.svelte`

## Verification

- `npm run check` in `frontend/`

## Notes

- Switched the QR preview from incremental updates to explicit redraws when options change, which makes style and color changes reliable.
- Tightened the section layout so the preview and controls use the container space more evenly.
- Frontend-only bugfix/polish slice.
