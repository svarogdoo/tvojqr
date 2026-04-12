# BRIEF

## User-Facing Goal

Clicking sign in on the live site should go to the real backend auth endpoint, not a broken relative path.

## Success Criteria

- Production sign-in uses a valid absolute backend URL.
- Missing env values no longer produce `undefined/...` URLs.

## UX Notes

- None; config/runtime bugfix.

## Risks Or Tradeoffs

- If Netlify env is still missing at build time, the frontend needs a safe fallback rather than failing open.
