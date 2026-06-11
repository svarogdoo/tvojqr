# BRIEF

## User-Facing Goal

Prevent accidental loss of project edits by making unsaved changes visible and interrupting accidental navigation away.

## Success Criteria

- Save area indicates when there are unsaved changes.
- Changes to name, slug, background color, draft uploads, and marked deletions are detected.
- Browser refresh/close warns when unsaved changes exist.
- In-app navigation warns when unsaved changes exist.
- Successful save redirects to dashboard and shows a success snackbar there.

## UX Notes

Use a straightforward warning and a stronger save CTA instead of adding a complex custom modal.

## Risks Or Tradeoffs

Browser refresh/close warning copy is controlled by the browser.
