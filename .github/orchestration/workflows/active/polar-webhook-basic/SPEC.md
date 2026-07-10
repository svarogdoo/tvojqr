# Spec

Scope:
- Add `Polar__WebhookSecret` config.
- Add `POST /api/billing/polar/webhook`.
- Verify Standard Webhooks headers.
- Handle `order.paid`, `subscription.active`, `subscription.created`, `subscription.updated`, `subscription.canceled`, and `subscription.revoked`.
- Add entitlement repository upsert support.

Out of scope:
- Customer billing portal.
- Full billing event ledger.
- Advanced downgrade/refund handling.
