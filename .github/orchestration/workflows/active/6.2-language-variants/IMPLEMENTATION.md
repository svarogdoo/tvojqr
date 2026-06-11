# IMPLEMENTATION

## Planned Changes

- Add language variant persistence.
- Add language add/remove API.
- Make uploads language-aware.
- Update editor sections.
- Update public selector/filtering.

## Files Touched

- `backend/src/HostingQr.Infrastructure/Migrations/Scripts/005_project_language_variants.sql`
- `backend/src/HostingQr.Domain/Projects/ProjectLanguageVariant.cs`
- `backend/src/HostingQr.Application/Abstractions/IProjectLanguageVariantRepository.cs`
- `backend/src/HostingQr.Infrastructure/Projects/ProjectLanguageVariantRepository.cs`
- `backend/src/HostingQr.Application/Projects/ProjectContracts.cs`
- `backend/src/HostingQr.Application/Projects/ProjectService.cs`
- `backend/src/HostingQr.Application/Assets/AssetService.cs`
- `backend/src/HostingQr.Api/Endpoints/ProjectEndpoints.cs`
- `frontend/src/lib/types/projects.ts`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- `frontend/src/routes/[slug]/+page.svelte`
- `README.md`
- `backend/README.md`

## Verification

- `dotnet test backend/HostingQr.Backend.sln`
- `npm run check --prefix frontend`

## Notes

- Keep the first version focused on images.
- Existing `default` assets are migrated to `en`.
