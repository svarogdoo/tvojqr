# INTENT

## User Request

Fix the production sign-in URL bug where the app navigates to `https://hostingqr.com/undefined/api/auth/google`.

## Requested Outcome

Frontend should always resolve a valid API base URL in production and never emit an `undefined/...` auth URL.

## Constraints

- Keep the fix focused on frontend API base resolution.
- Verification is required.

## Open Questions

- None for this bugfix.
