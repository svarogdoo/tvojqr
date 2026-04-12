# IMPLEMENTATION

## Planned Changes

- Add project status to the schema and response models.
- Add disable and delete backend endpoints.
- Show status in dashboard rows.
- Add disable/delete controls with confirmation in the project page.

## Files Touched

- `backend/src/HostingQr.Domain/Projects/Project.cs`
- `backend/src/HostingQr.Domain/Projects/ProjectStatus.cs`
- `backend/src/HostingQr.Domain/Projects/ProjectWithSlug.cs`
- `backend/src/HostingQr.Application/Projects/ProjectContracts.cs`
- `backend/src/HostingQr.Application/Abstractions/IProjectRepository.cs`
- `backend/src/HostingQr.Application/Abstractions/IProjectService.cs`
- `backend/src/HostingQr.Application/Projects/ProjectService.cs`
- `backend/src/HostingQr.Infrastructure/Projects/ProjectRepository.cs`
- `backend/src/HostingQr.Infrastructure/Migrations/Scripts/003_project_status.sql`
- `backend/src/HostingQr.Api/Endpoints/ProjectEndpoints.cs`
- `frontend/src/lib/types/projects.ts`
- `frontend/src/routes/dashboard/+page.svelte`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- `backend/tests/HostingQr.Api.Tests/ProjectEndpointTests.cs`

## Verification

- `dotnet test "backend/HostingQr.Backend.sln"`
- `npm run check` in `frontend/`

## Notes

- Added a simple active/disabled project status.
- Dashboard rows now show status and project pages include disable/delete controls.
- Delete requires explicit browser confirmation before removal.
- This slice focuses on status visibility and project lifecycle controls.
