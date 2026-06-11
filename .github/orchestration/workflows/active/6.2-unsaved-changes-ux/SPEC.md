# SPEC

## Scope

Add dirty-state tracking and leave warnings to the dashboard project editor.

## Affected Areas

- `frontend/src/routes/dashboard/projects/[projectId]/+page.svelte`
- README task list

## Technical Approach

- Track an initial saved form snapshot.
- Derive `hasUnsavedChanges` from form values, draft assets, and marked deletions.
- Use `beforeNavigate` for in-app navigation confirmation.
- Use `beforeunload` for tab refresh/close warning.
- Move save-success snackbar after `goto("/dashboard")` resolves.

## Assumptions

- Native `confirm()` is acceptable for this pass.

## Risks

- Native confirm dialogs are browser-styled and not brand-customized.
