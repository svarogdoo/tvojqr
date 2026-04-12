# BRIEF

## User-Facing Goal

Get the deployed backend to start cleanly with migrations enabled.

## Success Criteria

- Startup no longer fails at DB connection open when migrations are enabled.
- Railway internal DB connectivity is handled correctly.

## UX Notes

- None; runtime infrastructure bugfix.

## Risks Or Tradeoffs

- Environment-specific DB SSL assumptions can be subtle, so the fix should be explicit and narrow.
