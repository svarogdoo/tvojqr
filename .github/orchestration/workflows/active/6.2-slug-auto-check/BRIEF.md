# BRIEF

## User-Facing Goal

Users get slug availability feedback automatically without needing to click Check every time.

## Success Criteria

- Slug availability checks after typing pauses.
- Empty/current slugs are not repeatedly checked.
- Check and Random controls sit beside the URL field on larger screens.
- Existing manual check and random generation still work.

## UX Notes

Use a modest debounce so checks feel responsive without firing on every keystroke.

## Risks Or Tradeoffs

Native API calls may still overlap in edge cases, so stale auto-check responses should not overwrite newer input state.
