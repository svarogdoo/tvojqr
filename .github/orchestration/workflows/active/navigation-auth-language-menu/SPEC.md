# SPEC

## Scope

Update the navigation component to replace the language button row with a dropdown and add a compact account menu tied to auth state.

## Affected Areas

- `frontend/src/lib/components/Navigation.svelte`
- auth store usage already present in the component

## Technical Approach

- Use local component state for menu open/close behavior.
- Render one language trigger with a dropdown list of available languages.
- Render one account trigger with different menu actions based on auth state.
- Keep existing links and auth actions intact.

## Assumptions

- Desktop-focused hover behavior is acceptable for this slice.
- Mobile nav restructuring is not part of this task.

## Risks

- Dropdowns must remain readable and not conflict with the fixed nav layout.
