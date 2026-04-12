# BRIEF

## User-Facing Goal

When a project is deleted, its public URL should behave as missing rather than looking like a disabled project.

## Success Criteria

- Deleted projects do not resolve as disabled.
- Public slug lookup for deleted projects returns the missing-state path.

## UX Notes

- Keep disabled vs deleted meaning clearly separated.

## Risks Or Tradeoffs

- Existing local DB state may contain old rows from before recent schema/behavior changes.
