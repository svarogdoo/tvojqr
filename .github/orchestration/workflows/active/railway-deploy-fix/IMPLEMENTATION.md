# IMPLEMENTATION

## Planned Changes

- Add explicit Railway deployment config for the backend.
- Keep runtime assumptions production-safe.

## Files Touched

- `backend/railway.toml`
- `backend/nixpacks.toml`
- `backend/Dockerfile`
- `backend/.dockerignore`

## Verification

- backend build still passes locally

## Notes

- Added explicit Railway and Nixpacks build/start config.
- Added a backend Dockerfile as the most reliable Railway fallback when automatic build-plan detection fails.
