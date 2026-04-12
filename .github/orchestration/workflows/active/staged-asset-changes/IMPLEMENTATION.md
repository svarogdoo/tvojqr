# IMPLEMENTATION

## Planned Changes

- Add staged asset state to the project editor.
- Defer upload/delete backend mutations until save.

## Files Touched

- `frontend/src/routes/dashboard/+page.svelte`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`

## Verification

- `npm run check` in `frontend/`
- `dotnet test "backend/HostingQr.Backend.sln"`

## Notes

- Upload and delete actions now stage in local editor state first.
- Final save applies staged saved-asset deletions and staged uploads in sequence.
- Frontend-heavy editor flow refinement.
