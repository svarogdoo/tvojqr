# BRIEF

## User-Facing Goal

Users should sign in against the deployed backend and return to the deployed frontend, not localhost.

## Success Criteria

- Frontend API calls use the intended env-configured backend URL.
- Local development still works with localhost fallback.
- Deployment docs clearly state which frontend and backend env vars must be set.

## UX Notes

- None; config/deployment correctness slice.

## Risks Or Tradeoffs

- Backend redirect behavior still depends on production env values outside the repo.
