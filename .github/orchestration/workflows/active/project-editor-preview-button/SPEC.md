# SPEC

## Scope

Add a public-page preview button to the project editor header.

## Affected Areas

- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- `README.md` task tracking

## Technical Approach

- Reuse the dashboard eye icon and link pattern.
- Render the button next to the project title.
- Only render it when the project has a saved slug.

## Assumptions

- The saved slug is the correct public URL target for previewing.

## Risks

- Unsaved draft state may not have a previewable public URL.
