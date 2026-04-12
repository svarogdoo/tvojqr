# BRIEF

## User-Facing Goal

Google sign-in should work on the live site without redirect URI mismatch.

## Success Criteria

- The auth request uses `https://api.hostingqr.com/api/auth/google/callback`.
- The backend respects Railway proxy headers.

## UX Notes

- None; infrastructure/auth correctness slice.

## Risks Or Tradeoffs

- Proxy header configuration must be correct without opening unnecessary trust assumptions.
