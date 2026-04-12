# SPEC

## Scope

Fix Railway DB connection resolution so migrations can connect to internal Postgres.

## Affected Areas

- backend DB connection string resolver
- backend DB resolver tests

## Technical Approach

- Review current resolver defaults for SSL mode.
- Detect Railway internal hosts and avoid forcing incompatible SSL behavior.
- Keep external/public DB URLs working.

## Assumptions

- The failure occurs during connection open, before migration SQL, so the issue is connection config rather than migration content.

## Risks

- Different Railway DB URL shapes may require conservative fallback behavior.
