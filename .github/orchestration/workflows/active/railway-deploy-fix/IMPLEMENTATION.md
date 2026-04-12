# IMPLEMENTATION

## Planned Changes

- Add explicit Railway deployment config for the backend.
- Keep runtime assumptions production-safe.

## Files Touched

- `backend/railway.toml`

## Verification

- backend build still passes locally

## Notes

- Added an explicit Railway build/start plan for the backend using `dotnet publish` and a direct `dotnet out/HostingQr.Api.dll` start command.
