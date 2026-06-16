# BRIEF

## User-Facing Goal

The footer should sit at the bottom of the viewport on short pages, with no extra background visible beneath it.

## Success Criteria

- Footer is the last visible element.
- Short pages fill the viewport cleanly.
- The issue is fixed on mobile and desktop.

## UX Notes

- Keep the existing page palette and spacing.

## Risks Or Tradeoffs

- Any page that still uses `min-h-screen` incorrectly could reintroduce the gap.
