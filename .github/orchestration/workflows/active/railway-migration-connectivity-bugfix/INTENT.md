# INTENT

## User Request

Diagnose why turning migrations on in Railway causes DB connection failure during startup and fix the underlying connectivity assumption.

## Requested Outcome

Startup migrations should be able to connect to Railway Postgres when the backend is running inside Railway.

## Constraints

- Focus on DB connectivity assumptions, not migration script content.
- Verification is required.

## Open Questions

- None for this bugfix slice.
