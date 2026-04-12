# SPEC

## Scope

Fix overflow and narrow-screen layout issues on the project editor page.

## Affected Areas

- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- `frontend/src/lib/components/ProjectQrBuilder.svelte`

## Technical Approach

- Inspect long horizontal rows and replace them with stronger stacking/wrapping behavior.
- Reduce minimum widths that force overflow.
- Let action groups go full-width on small screens where appropriate.

## Assumptions

- Navigation/mobile menu work remains out of scope.

## Risks

- Too much stacking can reduce desktop compactness, so keep changes targeted.
