# SPEC

## Scope

Add a backend billing portal endpoint and frontend account page with a manage-billing action.

## Affected Areas

- Backend billing endpoints.
- Backend endpoint tests.
- Frontend account route.
- Navigation user menu.
- Billing setup docs and README task statuses.

## Technical Approach

- Add `POST /api/billing/portal`, protected by auth.
- Use Polar `POST /v1/customer-sessions/` with `external_customer_id` set to the current app user ID and `return_url` set to `/account` on the configured frontend base URL.
- Return `{ portalUrl }` to the frontend from Polar's `customer_portal_url` response.
- Add `/account` page that refreshes auth, loads entitlement, displays account/plan data, and redirects to the portal on button click.
- Add user menu link to `/account`.

## Assumptions

- Polar customer records are created by prior checkout because checkout already sends `external_customer_id`.
- The same `Polar__AccessToken` can include `customer_sessions:write` in addition to checkout scopes.

## Risks

- If a user has never checked out, Polar may not have a customer for that external ID and portal creation may fail.
- Production token scope must be updated or the portal endpoint will return an upstream failure.
