# BRIEF

## User-Facing Goal

Users should be able to add and remove images freely while editing without immediately mutating the saved project until they confirm save.

## Success Criteria

- Image uploads are staged locally first.
- Image deletes are staged locally first.
- The visual asset list reflects the staged state.
- Only clicking save applies staged asset changes to the backend.

## UX Notes

- Keep the staged behavior clear but lightweight.
- Avoid making the page feel slower or more complex.

## Risks Or Tradeoffs

- Save flow becomes a little more complex because it must reconcile staged additions and removals.
