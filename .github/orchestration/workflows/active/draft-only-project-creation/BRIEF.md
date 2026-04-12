# BRIEF

## User-Facing Goal

Let users experiment with a new project draft without creating a backend record until they intentionally save.

## Success Criteria

- Clicking `New project` opens an unsaved draft editor.
- Title, slug, and selected images live only in frontend state until save.
- Selected images show local previews before save.
- Saving creates the project and uploads the selected images.

## UX Notes

- Unsaved draft state should feel intentional, not broken.
- The page should still be familiar to the current editor flow.

## Risks Or Tradeoffs

- Unsaved draft work is lost on refresh or tab close.
