# BRIEF

## Goal

Keep a backend billing webhook audit trail for support/debugging while Polar remains the source of truth for payments.

## Success Criteria

- Signed Polar webhook deliveries create `billing_events` rows.
- Valid entitlement-changing webhooks still update `user_entitlements`.
- Ignored signed webhooks are visible in `billing_events`.
- Duplicate provider event IDs do not break webhook handling.
- Backend tests cover event recording behavior.

## Non-Goals

- Customer billing portal.
- Admin event viewer UI.
- Full payment ledger/reconciliation system.
- Frontend changes.
