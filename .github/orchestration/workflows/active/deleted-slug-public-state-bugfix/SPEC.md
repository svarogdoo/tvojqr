# SPEC

## Scope

Diagnose and fix the public lookup behavior for deleted projects.

## Affected Areas

- backend delete path
- backend public slug lookup
- possibly local database cascade behavior

## Technical Approach

- Inspect whether delete truly removes the project and slug rows.
- Verify whether public lookup is returning cached or stale DB-backed results.
- Apply the smallest fix so deleted records are treated as missing.

## Assumptions

- Disabled projects should still return the disabled state.
- Deleted projects should return missing.

## Risks

- Local data created before recent migrations may expose edge cases.
