# SPEC

## Scope

Refine the dashboard and project editor presentation without changing backend behavior.

## Affected Areas

- `frontend/src/routes/dashboard/+page.svelte`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`

## Technical Approach

- Collapse the project editor into a single main content column.
- Add a public-view link/button to each dashboard row using the project slug.
- Use a simple inline SVG eye icon rather than introducing a new icon dependency.

## Assumptions

- The public page URL is still `/{slug}` on the frontend.

## Risks

- The dashboard row action needs to avoid interfering with the existing row click/open behavior.
