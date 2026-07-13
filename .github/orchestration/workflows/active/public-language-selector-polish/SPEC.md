# SPEC

## Scope

Update `frontend/src/routes/[slug]/+page.svelte` selector styling.

## Technical Approach

- Set a CSS custom property from `project.backgroundColor` on the page shell.
- Use `color-mix()` to derive selector and dropdown colors from the page background.
- Normalize selector/dropdown rounding with consistent radii.
- Keep existing state and click handling unchanged.

## Risks

- `color-mix()` support is modern-browser oriented, which is acceptable for current frontend styling.
