# INTENT

## User Request

Add `billing_events` so Polar webhook/payment activity is kept for later debugging and investigation.

## Requested Outcome

The backend should keep an audit trail of signed Polar webhook events, including enough user/tier/provider details to investigate paid access issues.

## Constraints

- Implement only the approved audit-trail scope.
- Do not build a full billing ledger or admin UI.
- Preserve current entitlement behavior.
- Invalid webhook signatures should not be recorded.

## Open Questions

- None for this pass.
