# IMPLEMENTATION

## Planned Changes

- Add an eye icon link beside the project title in the editor header.
- Point it at the current project's public URL and open it in a new tab.
- Update the README task list to reflect the new preview access.

## Files Touched

- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- `README.md`

## Verification

- `npm run check --prefix frontend`
- `npm run build --prefix frontend`

## Notes

- Keep the button hidden when no saved slug exists.
