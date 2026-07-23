# BRIEF

## User-Facing Goal

Measure aggregate website usage and referral sources without presenting an analytics consent popup.

## Success Criteria

- No analytics popup or Cookie settings button is shown.
- No Google Analytics script is loaded.
- Cloudflare's cookie-free beacon loads only when a valid public site token is configured.
- Existing GA cookies and consent state are removed for returning visitors.

## UX Notes

The replacement should be invisible to visitors and must not add layout or interaction changes.

## Risks Or Tradeoffs

Cloudflare Web Analytics does not currently report UTM query parameters or custom events. Browser blockers can also block its JavaScript beacon.
