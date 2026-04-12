# BRIEF

## User-Facing Goal

Users should be able to create or edit a project without losing typed values, clearly see uploaded image previews, and get reliable save feedback.

## Success Criteria

- New projects start with an empty required title field.
- Upload previews are visible in the project editor.
- Save success and failure use a snackbar instead of inline ad-hoc messaging.
- Successful save returns the user to the dashboard.
- Failed save leaves the form state intact.

## UX Notes

- Keep feedback lightweight and reusable.
- Preserve the current visual language.
- Avoid making the save flow feel destructive or surprising.

## Risks Or Tradeoffs

- Redirect-after-save means feedback timing needs to be intentional so the user still understands what happened.
