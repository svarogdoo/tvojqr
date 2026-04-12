# IMPLEMENTATION

## Planned Changes

- Add backend asset-aware public response.
- Differentiate disabled and missing project states.
- Replace demo slug page with a real image-column public page.

## Files Touched

- `backend/src/HostingQr.Domain/Projects/PublicProject.cs`
- `backend/src/HostingQr.Application/Projects/ProjectContracts.cs`
- `backend/src/HostingQr.Application/Projects/ProjectService.cs`
- `backend/src/HostingQr.Infrastructure/Projects/ProjectRepository.cs`
- `backend/src/HostingQr.Api/Endpoints/PublicEndpoints.cs`
- `frontend/src/routes/[slug]/+page.svelte`
- `backend/tests/HostingQr.Api.Tests/ProjectEndpointTests.cs`

## Verification

- `dotnet test "backend/HostingQr.Backend.sln"`
- `npm run check` in `frontend/`

## Notes

- Public pages now use real backend data.
- Active projects render saved images in a vertical stack with small spacing.
- Disabled and missing states now have distinct branded messaging.
- This slice focuses only on default-language image presentation.
