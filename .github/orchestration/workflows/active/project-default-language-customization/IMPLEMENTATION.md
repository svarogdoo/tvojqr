# IMPLEMENTATION

## Planned Changes

- Add `defaultLanguageCode` and `defaultLanguageDisplayName` to project create/update requests.
- Seed new projects with the selected default language variant.
- Add repository support for changing the default language variant and updating matching assets.
- Add a default-language selector to the project editor.
- Update the README task list.

## Files Touched

- `backend/src/HostingQr.Application/Projects/ProjectContracts.cs`
- `backend/src/HostingQr.Application/Projects/ProjectService.cs`
- `backend/src/HostingQr.Application/Abstractions/IProjectLanguageVariantRepository.cs`
- `backend/src/HostingQr.Infrastructure/Projects/ProjectLanguageVariantRepository.cs`
- `backend/tests/HostingQr.Api.Tests/ProjectEndpointTests.cs`
- `frontend/src/lib/types/projects.ts`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- `README.md`

## Verification

- `npm run check --prefix frontend`
- `npm run build --prefix frontend`
- `dotnet test backend/HostingQr.Backend.sln`

## Notes

- Keep the change narrowly focused on the primary language only.
