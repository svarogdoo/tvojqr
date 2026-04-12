# SPEC

## Scope

Improve mobile layout behavior for the dashboard and project settings pages.

## Affected Areas

- `frontend/src/routes/dashboard/+page.svelte`
- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- possibly shared dashboard/project components used only by those pages

## Technical Approach

- Inspect current stacking, wrapping, and button density.
- Prefer vertical flow on narrow screens.
- Reduce cases where controls compete horizontally.
- Keep desktop behavior intact where possible.

## Assumptions

- Navigation/mobile menu restructuring is out of scope unless a local fix is necessary.

## Risks

- Tuning too many spacing values can create churn without much benefit, so keep fixes targeted.
