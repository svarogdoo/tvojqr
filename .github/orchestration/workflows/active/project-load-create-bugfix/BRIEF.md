# BRIEF

## User-Facing Goal

Users should be able to see correctly named projects, open them from the dashboard, and create a new one without hitting a broken state.

## Success Criteria

- Existing dashboard projects show their real names.
- Opening a project loads its settings page.
- Creating a new project works and redirects into the editor.
- Error states are reserved for genuine failures.

## UX Notes

- Keep the fix minimal and do not rework the surrounding dashboard UX unnecessarily.

## Risks Or Tradeoffs

- Existing local database state may interact with recent schema changes and expose data-shape issues.
