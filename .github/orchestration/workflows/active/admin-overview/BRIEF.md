# BRIEF

## Goal

Give the owner/admin a simple overview of platform usage and account tier distribution.

## Success Criteria

- `GET /api/admin/overview` returns metrics only for admin users.
- `/admin/overview` displays the metrics.
- Non-admin users cannot access admin metrics.
- Backend tests cover admin and non-admin access.

## Non-Goals

- Charts.
- User/account management UI.
- Billing event browsing.
- Date range filters.
