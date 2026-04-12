# IMPLEMENTATION

## Planned Changes

- Investigate the regression.
- Apply a minimal compatibility-safe fix.

## Files Touched

- `backend/src/HostingQr.Infrastructure/Configuration/StorageOptions.cs`
- `backend/src/HostingQr.Infrastructure/Assets/LocalAssetStorageService.cs`
- `backend/src/HostingQr.Infrastructure/DependencyInjection.cs`
- `backend/src/HostingQr.Api/Program.cs`
- `backend/src/HostingQr.Api/appsettings.Development.json`
- `backend/README.md`

## Verification

- `dotnet test "backend/HostingQr.Backend.sln"`
- `npm run check` in `frontend/`

## Notes

- Root cause: uploads were stored on local app disk, which is not reliable as persistent shared storage on Railway.
- Added support for a configurable persistent uploads root path so Railway can use a mounted volume.
- Bugfix only.
