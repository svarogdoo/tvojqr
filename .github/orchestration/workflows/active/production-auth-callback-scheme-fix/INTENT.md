# INTENT

## User Request

Fix the production Google auth callback so it uses `https://api.hostingqr.com/...` instead of `http://api.hostingqr.com/...`.

## Requested Outcome

The backend should generate the correct HTTPS callback URL behind Railway's proxy.

## Constraints

- Keep the fix focused on backend proxy/runtime behavior.
- Verification is required.

## Open Questions

- None for this slice.
