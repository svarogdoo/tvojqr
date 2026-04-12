# SPEC

## Scope

Refine the slug control presentation on the project page only.

## Affected Areas

- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`

## Technical Approach

- Keep the slug prefix, input, and actions grouped in one compact row.
- Move feedback into a more integrated inline status chip/row near the controls.
- Use clearer color treatment for available vs unavailable states.

## Assumptions

- The existing slug check and random generation behavior stays unchanged.

## Risks

- The row needs to wrap gracefully on smaller screens.
