# IMPLEMENTATION

## Planned Changes

- Add backend update endpoint/service for project name and slug.
- Add slug replacement persistence logic with one-primary-slug enforcement.
- Convert the project detail shell into an editable settings form.
- Add slug availability checks, random slug button, and save action.

## Files Touched

- `backend/src/HostingQr.Application/Projects/ProjectContracts.cs`
- `backend/src/HostingQr.Application/Abstractions/IProjectService.cs`
- `backend/src/HostingQr.Application/Abstractions/IProjectRepository.cs`
- `backend/src/HostingQr.Application/Abstractions/ISlugRepository.cs`
- `backend/src/HostingQr.Application/Abstractions/ISlugService.cs`
- `backend/src/HostingQr.Application/Projects/ProjectService.cs`
- `backend/src/HostingQr.Application/Slugs/SlugService.cs`
- `backend/src/HostingQr.Infrastructure/Projects/ProjectRepository.cs`
- `backend/src/HostingQr.Infrastructure/Slugs/SlugRepository.cs`
- `backend/src/HostingQr.Api/Endpoints/ProjectEndpoints.cs`
- `backend/tests/HostingQr.Api.Tests/ProjectEndpointTests.cs`
- `backend/tests/HostingQr.Api.Tests/SlugServiceTests.cs`
- `frontend/src/lib/types/projects.ts`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`

## Verification

- `dotnet test "backend/HostingQr.Backend.sln"`
- `npm run check` in `frontend/`

## Notes

- The backend update path keeps one active slug per project by updating the existing primary slug row.
- The frontend now supports name editing, custom slug checks, random slug generation, and save.
- This task intentionally stops before uploads/languages/preview.
