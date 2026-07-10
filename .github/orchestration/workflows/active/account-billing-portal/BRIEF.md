# BRIEF

## Goal

Give clients a self-service billing management path from inside HostingQr.

## Success Criteria

- User menu links to Account & billing.
- `/account` shows signed-in account details and current plan state.
- Authenticated users can request a fresh Polar portal link.
- Backend creates Polar customer sessions using the app user ID as `external_customer_id`.
- Build/tests verify the flow compiles and endpoint behavior is covered.

## Non-Goals

- In-app cancellation UI.
- Invoice list or billing history UI.
- Plan upgrade/downgrade UI.
- Admin billing event viewer.
