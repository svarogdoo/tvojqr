# BRIEF

## User-Facing Goal

Let an authenticated user open one project and immediately update the basic settings that define its hosted page identity.

## Success Criteria

- The project detail page shows editable fields for project name and slug.
- The user can check whether a custom slug is available.
- The user can click a button to generate a random slug.
- The user can save the updated settings successfully.
- The page shows clear loading, success, and error states.

## UX Notes

- Keep the form calm and aligned with the current dashboard styling.
- Slug helper states should be easy to understand.
- Avoid overbuilding the page before uploads/languages are added.

## Risks Or Tradeoffs

- The backend currently has create/list/detail but not update, so this slice requires a matching API addition.
