# IMPLEMENTATION

## Planned Changes

- Add migration and project model fields.
- Update project create/update/read paths.
- Add dashboard color controls.
- Apply color on public page.
- Update docs and verify.

## Files Touched

- `backend/src/HostingQr.Infrastructure/Migrations/Scripts/004_project_background_color.sql`
- `backend/src/HostingQr.Domain/Projects/ProjectWithSlug.cs`
- `backend/src/HostingQr.Domain/Projects/PublicProject.cs`
- `backend/src/HostingQr.Application/Abstractions/IProjectRepository.cs`
- `backend/src/HostingQr.Application/Projects/ProjectContracts.cs`
- `backend/src/HostingQr.Application/Projects/ProjectService.cs`
- `backend/src/HostingQr.Infrastructure/Projects/ProjectRepository.cs`
- `backend/tests/HostingQr.Api.Tests/ProjectEndpointTests.cs`
- `frontend/src/lib/types/projects.ts`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- `frontend/src/routes/[slug]/+page.svelte`
- `README.md`

## Verification

- `dotnet test backend/HostingQr.Backend.sln`
- `npm run check --prefix frontend`

## Notes

- Scope excludes broader theming.
- Default background color is `#f8f7f3`.
