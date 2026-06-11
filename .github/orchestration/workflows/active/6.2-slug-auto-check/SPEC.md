# SPEC

## Scope

Improve project editor slug availability checking and desktop layout.

## Affected Areas

- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- README task list

## Technical Approach

- Add a debounce timer for slug input.
- Reuse the existing availability endpoint.
- Track request order so stale responses do not overwrite newer input.
- Use responsive flex layout for desktop controls.

## Assumptions

- A debounce around 700ms is long enough to avoid noisy checks.

## Risks

- Availability status can still change later before save; backend conflict handling remains the source of truth.
