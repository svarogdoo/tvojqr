# IMPLEMENTATION

## Planned Changes

- Change dashboard new-project navigation to open a draft editor.
- Add draft-mode state and local image previews to the project editor.
- Save project first, then upload draft images.

## Files Touched

- `frontend/src/routes/dashboard/+page.svelte`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`

## Verification

- `npm run check` in `frontend/`
- `dotnet test "backend/HostingQr.Backend.sln"`

## Notes

- `New project` now opens `/dashboard/projects/new` as a frontend-only draft.
- Draft images stay in browser memory and use local object URLs for preview.
- Project creation and image upload now happen only on explicit save.
- This task is mainly frontend flow refinement on top of the existing backend.
