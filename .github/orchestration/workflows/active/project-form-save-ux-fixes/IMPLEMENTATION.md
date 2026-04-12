# IMPLEMENTATION

## Planned Changes

- Stabilize project form state and validation behavior.
- Fix uploaded image preview visibility.
- Add reusable snackbar infrastructure and use it for save/upload feedback.
- Redirect to dashboard after successful save.

## Files Touched

- `backend/src/HostingQr.Application/Abstractions/ICurrentUserContext.cs`
- `backend/src/HostingQr.Application/Projects/ProjectService.cs`
- `backend/src/HostingQr.Infrastructure/Auth/AuthenticatedUserContext.cs`
- `backend/src/HostingQr.Infrastructure/Users/DevelopmentUserContext.cs`
- `frontend/src/lib/components/Snackbar.svelte`
- `frontend/src/lib/stores/snackbar.ts`
- `frontend/src/routes/+layout.svelte`
- `frontend/src/routes/dashboard/+page.svelte`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`

## Verification

- `npm run check` in `frontend/`
- `dotnet test "backend/HostingQr.Backend.sln"`

## Notes

- New projects now start with an empty title value while the editor requires a title before save.
- Upload refresh no longer wipes unsaved form values.
- Save/upload feedback now uses a shared snackbar component.
- Successful save redirects back to the dashboard.
- This task is focused on UX correctness, not new product scope.
