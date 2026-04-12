# IMPLEMENTATION

## Planned Changes

- Add backend asset deletion.
- Add frontend image-card delete action.

## Files Touched

- `backend/src/HostingQr.Application/Abstractions/IAssetRepository.cs`
- `backend/src/HostingQr.Application/Abstractions/IAssetStorageService.cs`
- `backend/src/HostingQr.Application/Abstractions/IAssetService.cs`
- `backend/src/HostingQr.Application/Assets/AssetService.cs`
- `backend/src/HostingQr.Infrastructure/Assets/AssetRepository.cs`
- `backend/src/HostingQr.Infrastructure/Assets/LocalAssetStorageService.cs`
- `backend/src/HostingQr.Api/Endpoints/ProjectEndpoints.cs`
- `backend/tests/HostingQr.Api.Tests/ProjectEndpointTests.cs`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`

## Verification

- `dotnet test "backend/HostingQr.Backend.sln"`
- `npm run check` in `frontend/`

## Notes

- Added backend single-asset deletion with file cleanup.
- Added per-image delete controls for both draft and saved image cards.
- Focused asset-management slice.
