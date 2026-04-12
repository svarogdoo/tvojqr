# IMPLEMENTATION

## Planned Changes

- Fix frontend API base resolution.

## Files Touched

- `frontend/src/lib/config.ts`
- `frontend/src/lib/api.ts`

## Verification

- `npm run check` in `frontend/`

## Notes

- Switched API base resolution to runtime public env access via `$env/dynamic/public`.
- Kept the one-time browser console log on the first backend call so the resolved API URL can be inspected easily.
