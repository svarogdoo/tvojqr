# SPEC

## Scope

Add a database table and backend repository for billing webhook event records, then write audit rows from the Polar webhook endpoint after signature verification.

## Affected Areas

- Backend migrations.
- Billing webhook endpoint.
- Application repository abstraction.
- Infrastructure repository registration.
- Billing webhook endpoint tests.
- README task/status notes.

## Technical Approach

- Add `billing_events` with provider event ID, event type, optional user/tier/provider object IDs, processed action, entitlement result, raw JSON payload, and timestamps.
- Add `IBillingEventRepository` and Dapper implementation.
- Parse optional provider object IDs from Polar `data`.
- Record signed events before returning `Accepted`, using `on conflict do nothing` for duplicate provider event IDs.
- Keep current entitlement update path unchanged apart from adding event recording.

## Assumptions

- Webhook ID from the `webhook-id` header is the provider delivery/event identifier available to this endpoint.
- `raw_payload` may contain customer/provider data and is appropriate for backend-only audit storage.
- Existing tests can use a fake billing event repository instead of a real database.

## Risks

- Raw payload storage increases data retention/privacy responsibility.
- Polar payload fields may vary by event type, so provider object IDs are best-effort.
