# IMPLEMENTATION

## Planned Changes

- Add backend asset schema and upload endpoint.
- Add simple local file storage service for development uploads.
- Extend project detail responses to include current assets.
- Add frontend upload UI to the project page.

## Files Touched

- `backend/src/HostingQr.Domain/Assets/Asset.cs`
- `backend/src/HostingQr.Application/Assets/AssetContracts.cs`
- `backend/src/HostingQr.Application/Assets/AssetService.cs`
- `backend/src/HostingQr.Application/Abstractions/IAssetRepository.cs`
- `backend/src/HostingQr.Application/Abstractions/IAssetService.cs`
- `backend/src/HostingQr.Application/Abstractions/IAssetStorageService.cs`
- `backend/src/HostingQr.Application/Projects/ProjectContracts.cs`
- `backend/src/HostingQr.Application/Projects/ProjectService.cs`
- `backend/src/HostingQr.Application/DependencyInjection.cs`
- `backend/src/HostingQr.Application/HostingQr.Application.csproj`
- `backend/src/HostingQr.Infrastructure/Assets/AssetRepository.cs`
- `backend/src/HostingQr.Infrastructure/Assets/LocalAssetStorageService.cs`
- `backend/src/HostingQr.Infrastructure/DependencyInjection.cs`
- `backend/src/HostingQr.Infrastructure/Migrations/Scripts/001_initial.sql`
- `backend/src/HostingQr.Infrastructure/Migrations/Scripts/002_assets.sql`
- `backend/src/HostingQr.Api/Endpoints/ProjectEndpoints.cs`
- `backend/src/HostingQr.Api/Program.cs`
- `frontend/src/lib/api.ts`
- `frontend/src/lib/types/projects.ts`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- `backend/tests/HostingQr.Api.Tests/ProjectEndpointTests.cs`

## Verification

- `dotnet test "backend/HostingQr.Backend.sln"`
- `npm run check` in `frontend/`

## Notes

- Uploaded files are stored locally under the backend web root for now.
- The current upload flow is image-only and default-language-only.
- Project detail responses now include current asset metadata so the dashboard can render uploaded images.
- This slice intentionally stops before per-language uploads, preview, and final publish flow.
