# SPEC

## Scope

Add admin overview backend endpoint and frontend page.

## Affected Areas

- Backend admin endpoint.
- Application/infrastructure admin overview query.
- Frontend admin page.
- Tests and docs.

## Technical Approach

- Add `AdminOverviewResponse` with total accounts, total views, and accounts by tier.
- Add repository abstraction and Dapper implementation for aggregate metrics.
- Protect `/api/admin/overview` with auth plus entitlement tier `admin` check.
- Add `/admin/overview` page that loads entitlement first and then metrics.

## Assumptions

- Accounts with no active entitlement count as `none`.
- `project_view_counts.total_views` is the source for total views.

## Risks

- Admin frontend route is client-side guarded; backend remains the source of truth for access control.
