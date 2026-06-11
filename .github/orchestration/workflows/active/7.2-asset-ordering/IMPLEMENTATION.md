# IMPLEMENTATION

## Planned Changes

- Add asset reorder contract and endpoint.
- Add repository update for sort order.
- Add saved-image drag/drop UI.
- Persist order during save.
- Verify backend and frontend.

## Files Touched

- `backend/src/HostingQr.Application/Assets/AssetContracts.cs`
- `backend/src/HostingQr.Application/Abstractions/IAssetRepository.cs`
- `backend/src/HostingQr.Application/Abstractions/IAssetService.cs`
- `backend/src/HostingQr.Application/Assets/AssetService.cs`
- `backend/src/HostingQr.Infrastructure/Assets/AssetRepository.cs`
- `backend/src/HostingQr.Api/Endpoints/ProjectEndpoints.cs`
- `backend/tests/HostingQr.Api.Tests/ProjectEndpointTests.cs`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- `README.md`

## Verification

- `dotnet test backend/HostingQr.Backend.sln`
- `npm run check --prefix frontend`

## Notes

- Draft image ordering is out of scope for this pass.
- Public page ordering uses the existing `sort_order` mapping.
